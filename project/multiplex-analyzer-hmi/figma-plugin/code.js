// Generates the MultiPlex Analyzer HMI design system in the current Figma file:
// variables (Light/Dark), text styles, components with variants, and four screens.
// Names match docs/design-token-map.md and Themes/*.xaml one-to-one.

const COLOR_TOKENS = [
  ['color/surface/base', '#FFFFFF', '#161616'],
  ['color/surface/raised', '#F4F4F4', '#262626'],
  ['color/surface/sunken', '#E0E0E0', '#0B0B0B'],
  ['color/text/primary', '#161616', '#F4F4F4'],
  ['color/text/secondary', '#525252', '#A8A8A8'],
  ['color/text/disabled', '#A8A8A8', '#6F6F6F'],
  ['color/text/on-accent', '#FFFFFF', '#FFFFFF'],
  ['color/border/subtle', '#E0E0E0', '#393939'],
  ['color/border/strong', '#8D8D8D', '#6F6F6F'],
  ['color/accent/default', '#0F62FE', '#4589FF'],
  ['color/accent/hover', '#0353E9', '#78A9FF'],
  ['color/accent/pressed', '#002D9C', '#0F62FE'],
  ['color/status/idle', '#8D8D8D', '#6F6F6F'],
  ['color/status/running', '#0F62FE', '#4589FF'],
  ['color/status/paused', '#D2A106', '#F1C21B'],
  ['color/status/error', '#DA1E28', '#FA4D56'],
  ['color/status/ok', '#198038', '#42BE65'],
];

const LAYOUT_TOKENS = [
  ['space/1', 4], ['space/2', 8], ['space/3', 12], ['space/4', 16], ['space/5', 24], ['space/6', 32],
  ['radius/s', 4], ['radius/m', 8], ['radius/l', 16],
  ['touch/min', 48],
];

const TEXT_STYLES = [
  ['Display', 32, 'semibold'],
  ['Title', 20, 'semibold'],
  ['Body', 16, 'regular'],
  ['Caption', 13, 'regular'],
];

const C = {};
const CD = {};
const L = {};
const T = {};
const FONT = { regular: null, semibold: null };
const lightToDark = {};

// ---------- primitives ----------

function hexToRgb(hex) {
  const n = parseInt(hex.slice(1), 16);
  return { r: ((n >> 16) & 255) / 255, g: ((n >> 8) & 255) / 255, b: (n & 255) / 255 };
}

function paint(tokenName) {
  const base = { type: 'SOLID', color: { r: 0, g: 0, b: 0 } };
  return [figma.variables.setBoundVariableForPaint(base, 'color', C[tokenName])];
}

function bindRadius(node, tokenName) {
  for (const field of ['topLeftRadius', 'topRightRadius', 'bottomLeftRadius', 'bottomRightRadius']) {
    node.setBoundVariable(field, L[tokenName]);
  }
}

function rect(w, h, fill, opts = {}) {
  const r = figma.createRectangle();
  r.resize(w, h);
  r.fills = fill ? paint(fill) : [];
  if (opts.radius) bindRadius(r, opts.radius);
  if (opts.stroke) { r.strokes = paint(opts.stroke); r.strokeWeight = opts.strokeWeight || 1; }
  if (opts.name) r.name = opts.name;
  return r;
}

function ellipse(d, fill, opts = {}) {
  const e = figma.createEllipse();
  e.resize(d, d);
  e.fills = fill ? paint(fill) : [];
  if (opts.stroke) { e.strokes = paint(opts.stroke); e.strokeWeight = opts.strokeWeight || 1; }
  if (opts.name) e.name = opts.name;
  return e;
}

function tryBind(node, field, variable) {
  try { node.setBoundVariable(field, variable); } catch (_) { /* field not bindable in this Figma version */ }
}

async function text(str, styleName, fill, opts = {}) {
  const t = figma.createText();
  const style = T[styleName];
  t.fontName = style.fontName;
  t.characters = str;
  await t.setTextStyleIdAsync(style.id);
  t.fills = paint(fill || 'color/text/primary');
  if (opts.align) t.textAlignHorizontal = opts.align;
  if (opts.name) t.name = opts.name;
  return t;
}

function frame(name, w, h, opts = {}) {
  const f = figma.createFrame();
  f.name = name;
  f.resize(w, h);
  f.fills = opts.fill ? paint(opts.fill) : [];
  if (opts.radius) bindRadius(f, opts.radius);
  if (opts.stroke) { f.strokes = paint(opts.stroke); f.strokeWeight = 1; }
  if (opts.clip !== undefined) f.clipsContent = opts.clip;
  return f;
}

function autoLayout(f, direction, opts = {}) {
  f.layoutMode = direction;
  f.primaryAxisSizingMode = opts.primary || 'AUTO';
  f.counterAxisSizingMode = opts.counter || 'AUTO';
  const pad = opts.pad === undefined ? 0 : opts.pad;
  f.paddingLeft = f.paddingRight = opts.padX === undefined ? pad : opts.padX;
  f.paddingTop = f.paddingBottom = opts.padY === undefined ? pad : opts.padY;
  f.itemSpacing = opts.gap || 0;
  f.primaryAxisAlignItems = opts.mainAlign || 'MIN';
  f.counterAxisAlignItems = opts.crossAlign || 'CENTER';
  if (opts.gapToken) f.setBoundVariable('itemSpacing', L[opts.gapToken]);
  if (opts.padToken) {
    for (const field of ['paddingLeft', 'paddingRight', 'paddingTop', 'paddingBottom']) {
      f.setBoundVariable(field, L[opts.padToken]);
    }
  }
  return f;
}

function place(parent, node, x, y) {
  parent.appendChild(node);
  node.x = x;
  node.y = y;
  return node;
}

// ---------- fonts ----------

async function resolveFonts() {
  const available = await figma.listAvailableFontsAsync();
  const has = (family, style) => available.some(f => f.fontName.family === family && f.fontName.style === style);

  if (has('Segoe UI', 'Regular') && has('Segoe UI', 'Semibold')) {
    FONT.regular = { family: 'Segoe UI', style: 'Regular' };
    FONT.semibold = { family: 'Segoe UI', style: 'Semibold' };
  } else {
    FONT.regular = { family: 'Inter', style: 'Regular' };
    FONT.semibold = { family: 'Inter', style: 'Semi Bold' };
  }

  await figma.loadFontAsync(FONT.regular);
  await figma.loadFontAsync(FONT.semibold);
}

// ---------- 1. variables ----------

// Free plan allows one mode per collection, so Light and Dark are two collections
// with identical variable names. Existing collections/variables are reused on re-run.

async function getOrCreateCollection(name) {
  const all = await figma.variables.getLocalVariableCollectionsAsync();
  return all.find(c => c.name === name) || figma.variables.createVariableCollection(name);
}

async function getOrCreateVariable(name, collection, type, value) {
  const all = await figma.variables.getLocalVariablesAsync(type);
  let v = all.find(x => x.name === name && x.variableCollectionId === collection.id);
  if (!v) v = figma.variables.createVariable(name, collection, type);
  v.setValueForMode(collection.modes[0].modeId, value);
  return v;
}

async function createVariables() {
  const lightCollection = await getOrCreateCollection('Color / Light');
  const darkCollection = await getOrCreateCollection('Color / Dark');
  const layoutCollection = await getOrCreateCollection('Layout');

  for (const [name, light, dark] of COLOR_TOKENS) {
    C[name] = await getOrCreateVariable(name, lightCollection, 'COLOR', hexToRgb(light));
    CD[name] = await getOrCreateVariable(name, darkCollection, 'COLOR', hexToRgb(dark));
    lightToDark[C[name].id] = CD[name];
  }

  for (const [name, value] of LAYOUT_TOKENS) {
    L[name] = await getOrCreateVariable(name, layoutCollection, 'FLOAT', value);
  }
}

function swapPaints(paints) {
  return paints.map(p => {
    const id = p.boundVariables && p.boundVariables.color && p.boundVariables.color.id;
    if (!id || !lightToDark[id]) return p;
    return figma.variables.setBoundVariableForPaint(p, 'color', lightToDark[id]);
  });
}

function rebindToDark(root) {
  const nodes = [root, ...root.findAll(() => true)];
  for (const n of nodes) {
    try { if ('fills' in n && Array.isArray(n.fills)) n.fills = swapPaints(n.fills); } catch (_) { /* locked or mixed */ }
    try { if ('strokes' in n && Array.isArray(n.strokes)) n.strokes = swapPaints(n.strokes); } catch (_) { /* locked or mixed */ }
  }
}

// ---------- 2. text styles ----------

function createTextStyles() {
  for (const [name, size, weight] of TEXT_STYLES) {
    const s = figma.createTextStyle();
    s.name = name;
    s.fontName = FONT[weight];
    s.fontSize = size;
    T[name] = s;
  }
}

// ---------- 3. components ----------

async function buttonVariant(kind, state, label) {
  const f = frame(`State=${state}`, 140, 48, { radius: 'radius/m' });
  autoLayout(f, 'HORIZONTAL', { primary: 'AUTO', counter: 'FIXED', padX: 16, mainAlign: 'CENTER' });
  f.resize(140, 48);
  tryBind(f, 'height', L['touch/min']);
  try { f.minWidth = 120; } catch (_) { /* older API */ }

  let bg, fg, stroke = null;
  if (kind === 'Primary') {
    bg = { Default: 'color/accent/default', Hover: 'color/accent/hover', Pressed: 'color/accent/pressed', Disabled: 'color/surface/sunken' }[state];
    fg = state === 'Disabled' ? 'color/text/disabled' : 'color/text/on-accent';
  } else {
    bg = { Default: null, Hover: 'color/surface/raised', Pressed: 'color/surface/sunken', Disabled: null }[state];
    fg = state === 'Disabled' ? 'color/text/disabled' : 'color/text/primary';
    stroke = state === 'Disabled' ? 'color/border/subtle' : 'color/border/strong';
  }

  f.fills = bg ? paint(bg) : [];
  if (stroke) { f.strokes = paint(stroke); f.strokeWeight = 1; }

  const t = await text(label, 'Body', fg);
  t.fontName = FONT.semibold;
  f.appendChild(t);
  return figma.createComponentFromNode(f);
}

async function buildButtonSet(kind, label, page, y) {
  const variants = [];
  for (const state of ['Default', 'Hover', 'Pressed', 'Disabled']) {
    const c = await buttonVariant(kind, state, label);
    place(page, c, 0, y);
    variants.push(c);
  }
  const set = figma.combineAsVariants(variants, page);
  set.name = `Button / ${kind}`;
  set.x = 0; set.y = y;
  layoutSet(set);
  return set;
}

function layoutSet(set) {
  set.layoutMode = 'HORIZONTAL';
  set.itemSpacing = 24;
  set.paddingLeft = set.paddingRight = set.paddingTop = set.paddingBottom = 24;
  set.primaryAxisSizingMode = 'AUTO';
  set.counterAxisSizingMode = 'AUTO';
}

async function toggleVariant(state) {
  const f = frame(`State=${state}`, 52, 48);
  const track = rect(52, 28, state === 'On' ? 'color/accent/default' : 'color/surface/sunken', { name: 'Track' });
  track.cornerRadius = 14;
  if (state !== 'On') { track.strokes = paint('color/border/subtle'); track.strokeWeight = 1; }
  place(f, track, 0, 10);
  const thumb = ellipse(22, 'color/text/on-accent', { stroke: 'color/border/strong', name: 'Thumb' });
  place(f, thumb, state === 'On' ? 27 : 3, 13);
  if (state === 'Disabled') f.opacity = 0.4;
  return figma.createComponentFromNode(f);
}

async function buildToggleSet(page, y) {
  const variants = [];
  for (const state of ['Off', 'On', 'Disabled']) {
    const c = await toggleVariant(state);
    place(page, c, 0, y);
    variants.push(c);
  }
  const set = figma.combineAsVariants(variants, page);
  set.name = 'ToggleSwitch';
  set.x = 0; set.y = y;
  layoutSet(set);
  return set;
}

async function segmented(items, selectedIndex) {
  const f = frame('SegmentedControl', 10, 48, { fill: 'color/surface/sunken', radius: 'radius/m', stroke: 'color/border/subtle' });
  autoLayout(f, 'HORIZONTAL', { pad: 2, gap: 0 });
  for (let i = 0; i < items.length; i++) {
    const selected = i === selectedIndex;
    const item = frame('Item', 120, 40, { radius: 'radius/s' });
    autoLayout(item, 'HORIZONTAL', { primary: 'AUTO', counter: 'FIXED', padX: 16, mainAlign: 'CENTER' });
    item.resize(120, 40);
    try { item.minWidth = 120; } catch (_) { /* older API */ }
    item.fills = selected ? paint('color/accent/default') : [];
    const t = await text(items[i], 'Body', selected ? 'color/text/on-accent' : 'color/text/secondary');
    t.fontSize = 15;
    if (selected) t.fontName = FONT.semibold;
    item.appendChild(t);
    f.appendChild(item);
  }
  return f;
}

async function buildSegmented(page, y) {
  const f = await segmented(['Fast', 'Standard', 'High Sensitivity'], 1);
  const c = figma.createComponentFromNode(f);
  c.name = 'SegmentedControl';
  place(page, c, 0, y);
  return c;
}

async function stepperVariant(state, value, unit) {
  const f = frame(`State=${state}`, 200, 48, { fill: 'color/surface/base', radius: 'radius/m', stroke: 'color/border/strong' });
  autoLayout(f, 'HORIZONTAL', { primary: 'FIXED', counter: 'FIXED', mainAlign: 'SPACE_BETWEEN' });
  f.resize(200, 48);
  const minus = await text('-', 'Title', state === 'AtMin' ? 'color/text/disabled' : 'color/text/primary');
  minus.fontSize = 22; minus.textAlignHorizontal = 'CENTER'; minus.resize(48, 30);
  const middle = frame('Value', 10, 48); autoLayout(middle, 'HORIZONTAL', { gap: 4 });
  const v = await text(value, 'Body', 'color/text/primary'); v.fontSize = 18; v.fontName = FONT.semibold;
  const u = await text(unit, 'Caption', 'color/text/secondary');
  middle.appendChild(v); middle.appendChild(u);
  const plus = await text('+', 'Title', state === 'AtMax' ? 'color/text/disabled' : 'color/text/primary');
  plus.fontSize = 22; plus.textAlignHorizontal = 'CENTER'; plus.resize(48, 30);
  f.appendChild(minus); f.appendChild(middle); f.appendChild(plus);
  return figma.createComponentFromNode(f);
}

async function buildStepperSet(page, y) {
  const variants = [];
  for (const [state, value] of [['Default', '37'], ['AtMin', '30'], ['AtMax', '45']]) {
    const c = await stepperVariant(state, value, 'C');
    place(page, c, 0, y);
    variants.push(c);
  }
  const set = figma.combineAsVariants(variants, page);
  set.name = 'NumericStepper';
  set.x = 0; set.y = y;
  layoutSet(set);
  return set;
}

async function buildStatusDotSet(page, y) {
  const map = { Idle: 'color/status/idle', Running: 'color/status/running', Paused: 'color/status/paused', Completed: 'color/status/ok', Error: 'color/status/error' };
  const variants = [];
  for (const state of Object.keys(map)) {
    const f = frame(`State=${state}`, 14, 14);
    place(f, ellipse(14, map[state]), 0, 0);
    const c = figma.createComponentFromNode(f);
    place(page, c, 0, y);
    variants.push(c);
  }
  const set = figma.combineAsVariants(variants, page);
  set.name = 'StatusDot';
  set.x = 0; set.y = y;
  layoutSet(set);
  return set;
}

async function navItemVariant(state, label) {
  const selected = state === 'Selected';
  const f = frame(`State=${state}`, 88, 76, { fill: selected ? 'color/surface/base' : null });
  if (selected) place(f, rect(3, 76, 'color/accent/default', { name: 'Indicator' }), 0, 0);
  const icon = rect(22, 22, selected ? 'color/accent/default' : 'color/text/secondary', { name: 'Icon' });
  icon.cornerRadius = 3;
  place(f, icon, 33, 17);
  const t = await text(label, 'Caption', selected ? 'color/text/primary' : 'color/text/secondary', { align: 'CENTER' });
  t.fontSize = 11; t.resize(88, 14);
  place(f, t, 0, 47);
  return figma.createComponentFromNode(f);
}

async function buildNavItemSet(page, y) {
  const variants = [];
  for (const state of ['Default', 'Selected']) {
    const c = await navItemVariant(state, 'Dashboard');
    place(page, c, 0, y);
    variants.push(c);
  }
  const set = figma.combineAsVariants(variants, page);
  set.name = 'NavItem';
  set.x = 0; set.y = y;
  layoutSet(set);
  return set;
}

async function buildLevelPillSet(page, y) {
  const map = { Info: ['color/surface/sunken', 'color/text/secondary'], Warning: ['color/status/paused', 'color/text/on-accent'], Error: ['color/status/error', 'color/text/on-accent'] };
  const variants = [];
  for (const level of Object.keys(map)) {
    const f = frame(`Level=${level}`, 10, 20, { fill: map[level][0], radius: 'radius/s' });
    autoLayout(f, 'HORIZONTAL', { padX: 8, padY: 2 });
    const t = await text(level, 'Caption', map[level][1]);
    t.fontSize = 11; t.fontName = FONT.semibold;
    f.appendChild(t);
    const c = figma.createComponentFromNode(f);
    place(page, c, 0, y);
    variants.push(c);
  }
  const set = figma.combineAsVariants(variants, page);
  set.name = 'LevelPill';
  set.x = 0; set.y = y;
  layoutSet(set);
  return set;
}

// ---------- 4. screens ----------

function variantOf(set, prop, value) {
  const child = set.children.find(c => c.name === `${prop}=${value}`) || set.defaultVariant;
  return child.createInstance();
}

async function shell(name, K, selectedNav) {
  const f = frame(name, 1920, 1080, { fill: 'color/surface/base', clip: true });

  const sidebar = rect(88, 1080, 'color/surface/raised', { name: 'Sidebar' });
  place(f, sidebar, 0, 0);
  const navLabels = ['Dashboard', 'Plate', 'Settings', 'Log'];
  for (let i = 0; i < navLabels.length; i++) {
    const inst = variantOf(K.nav, 'State', i === selectedNav ? 'Selected' : 'Default');
    place(f, inst, 0, i * 76);
    const label = inst.findOne(n => n.type === 'TEXT');
    if (label) label.characters = navLabels[i];
  }

  const topbar = rect(1832, 64, 'color/surface/base', { name: 'TopBar' });
  place(f, topbar, 88, 0);
  place(f, rect(1832, 1, 'color/border/subtle'), 88, 64);
  place(f, variantOf(K.dot, 'State', selectedNav === 0 ? 'Running' : 'Idle'), 112, 25);
  place(f, await text(selectedNav === 0 ? 'Running' : 'Idle', 'Body'), 134, 22);
  place(f, rect(1, 20, 'color/border/subtle'), 230, 22);
  place(f, await text('Remaining', 'Caption', 'color/text/secondary'), 250, 24);
  place(f, await text(selectedNav === 0 ? '00:42' : '--:--', 'Body'), 330, 22);
  const theme = variantOf(K.secondary, 'State', 'Default');
  theme.resize(150, 40);
  const themeText = theme.findOne(n => n.type === 'TEXT');
  if (themeText) themeText.characters = 'Light';
  place(f, theme, 1746, 12);

  return f;
}

async function card(w, h, name) {
  return frame(name || 'Card', w, h, { fill: 'color/surface/raised', radius: 'radius/m', stroke: 'color/border/subtle' });
}

async function header(f, title, subtitle) {
  place(f, await text(title, 'Display'), 112, 88);
  place(f, await text(subtitle, 'Caption', 'color/text/secondary'), 112, 132);
}

async function dashboardScreen(K) {
  const f = await shell('Dashboard', K, 0);
  await header(f, 'Run Dashboard', 'Protocol state machine, hand-drawn progress ring, modal confirmation');

  const track = ellipse(260, null, { stroke: 'color/surface/sunken', strokeWeight: 18, name: 'Track' });
  track.arcData = { startingAngle: 0, endingAngle: Math.PI * 2, innerRadius: 0 };
  place(f, track, 700, 360);
  const arc = ellipse(260, 'color/accent/default', { name: 'Progress' });
  arc.arcData = { startingAngle: -Math.PI / 2, endingAngle: -Math.PI / 2 + Math.PI * 2 * 0.3, innerRadius: 0.862 };
  place(f, arc, 700, 360);
  const pct = await text('30%', 'Display', 'color/text/primary', { align: 'CENTER' });
  pct.fontSize = 52; pct.resize(260, 62);
  place(f, pct, 700, 448);
  const rem = await text('00:42', 'Caption', 'color/text/secondary', { align: 'CENTER' });
  rem.resize(260, 16);
  place(f, rem, 700, 514);
  place(f, variantOf(K.dot, 'State', 'Running'), 782, 660);
  place(f, await text('Running', 'Title'), 804, 654);

  place(f, await text('Protocol', 'Title'), 1364, 200);
  const steps = [['Load', '5 min', 'done'], ['Incubate', '30 min', 'active'], ['Wash', '10 min', ''], ['Read', '15 min', '']];
  for (let i = 0; i < steps.length; i++) {
    const [name, dur, st] = steps[i];
    const row = frame(`Step ${name}`, 380, 56, { fill: st === 'active' ? 'color/surface/raised' : 'color/surface/base', radius: 'radius/m', stroke: st === 'active' ? 'color/accent/default' : 'color/border/subtle' });
    place(f, row, 1364, 240 + i * 64);
    place(row, ellipse(12, st === 'done' ? 'color/status/ok' : st === 'active' ? 'color/accent/default' : 'color/border/strong'), 16, 22);
    place(row, await text(name, 'Body'), 42, 18);
    const d = await text(dur, 'Caption', 'color/text/secondary', { align: 'RIGHT' }); d.resize(80, 16);
    place(row, d, 284, 20);
  }

  const btns = [['primary', 'Start Run', 'Disabled'], ['secondary', 'Pause', 'Default'], ['secondary', 'Abort', 'Default'], ['secondary', 'Inject Error', 'Default']];
  let x = 112;
  for (const [kind, label, state] of btns) {
    const inst = variantOf(kind === 'primary' ? K.primary : K.secondary, 'State', state);
    const t = inst.findOne(n => n.type === 'TEXT'); if (t) t.characters = label;
    place(f, inst, x, 980);
    x += inst.width + 12;
  }
  return f;
}

async function plateScreen(K) {
  const f = await shell('Plate', K, 1);
  await header(f, 'Plate Map', '96 wells drawn in one OnRender pass. Pinch or scroll to zoom, drag to pan, tap a well for markers.');

  let x = 112;
  for (const label of ['-', '150%', '+', 'Reset view']) {
    if (label === '150%') { place(f, await text(label, 'Body'), x + 8, 187); x += 64; continue; }
    const b = variantOf(K.secondary, 'State', 'Default');
    b.resize(label.length > 2 ? 130 : 48, 40);
    const t = b.findOne(n => n.type === 'TEXT'); if (t) t.characters = label;
    place(f, b, x, 176); x += b.width + 8;
  }

  const plateArea = frame('PlateArea', 1380, 800, { fill: 'color/surface/raised', radius: 'radius/m', stroke: 'color/border/subtle', clip: true });
  place(f, plateArea, 112, 240);
  const body = rect(1180, 760, 'color/surface/base', { stroke: 'color/border/strong', strokeWeight: 1.5, name: 'Plate' });
  body.cornerRadius = 10;
  place(plateArea, body, 100, 40);
  const cell = 92;
  const ox = 130, oy = 90;
  for (let c = 0; c < 12; c++) {
    const t = await text(String(c + 1), 'Caption', 'color/text/secondary', { align: 'CENTER' }); t.resize(cell, 16);
    place(plateArea, t, ox + c * cell, oy - 30);
  }
  const hot = new Set(['0-3', '1-7', '2-2', '2-9', '3-5', '4-1', '4-10', '5-6', '6-4', '6-11', '7-0', '7-8']);
  for (let r = 0; r < 8; r++) {
    const t = await text(String.fromCharCode(65 + r), 'Caption', 'color/text/secondary', { align: 'RIGHT' }); t.resize(20, 16);
    place(plateArea, t, ox - 34, oy + r * cell + cell / 2 - 8);
    for (let c = 0; c < 12; c++) {
      const isHot = hot.has(`${r}-${c}`);
      const w = ellipse(70, isHot ? 'color/status/error' : 'color/surface/sunken', { stroke: 'color/border/strong', name: `${String.fromCharCode(65 + r)}${c + 1}` });
      if (isHot && (r + c) % 3 === 0) w.opacity = 0.55;
      place(plateArea, w, ox + c * cell + 11, oy + r * cell + 11);
    }
  }
  const ring = ellipse(80, null, { stroke: 'color/accent/default', strokeWeight: 3, name: 'Selection' });
  place(plateArea, ring, ox + 9 * cell + 6, oy + 2 * cell + 6);

  const panel = await card(340, 800, 'DetailPanel');
  place(f, panel, 1508, 240);
  place(panel, await text('C10', 'Display'), 24, 24);
  place(panel, await text('Well', 'Caption', 'color/text/secondary'), 284, 28);
  place(panel, await text('3 of 8 markers positive', 'Caption', 'color/text/secondary'), 24, 68);
  const markers = [0.82, 0.31, 0.67, 0.12, 0.45, 0.91, 0.28, 0.55];
  for (let i = 0; i < 8; i++) {
    const y = 110 + i * 34;
    place(panel, await text(`M-0${i + 1}`, 'Body'), 24, y);
    place(panel, rect(150, 10, 'color/surface/sunken'), 84, y + 6).cornerRadius = 5;
    const pos = markers[i] >= 0.6;
    place(panel, rect(Math.round(150 * markers[i]), 10, pos ? 'color/status/error' : 'color/border/strong'), 84, y + 6).cornerRadius = 5;
    place(panel, await text(markers[i].toFixed(2), 'Caption', 'color/text/secondary'), 244, y + 3);
    const pill = variantOf(K.pill, 'Level', pos ? 'Error' : 'Info');
    const pt = pill.findOne(n => n.type === 'TEXT'); if (pt) pt.characters = pos ? 'POS' : 'NEG';
    place(panel, pill, 288, y + 2);
  }
  const close = variantOf(K.secondary, 'State', 'Default');
  close.resize(292, 48);
  const ct = close.findOne(n => n.type === 'TEXT'); if (ct) ct.characters = 'Close';
  place(panel, close, 24, 728);
  return f;
}

async function settingsScreen(K) {
  const f = await shell('Settings', K, 2);
  await header(f, 'Settings', 'Custom controls, design tokens, theme switching');
  place(f, await text('Run parameters', 'Title'), 112, 200);

  const cardNode = await card(1696, 420, 'RunParameters');
  place(f, cardNode, 112, 240);
  const rows = [
    ['Read mode', 'Trade-off between scan time and signal sensitivity', 'segmented'],
    ['Incubation temperature', '30 - 45 C, 0.5 steps. Press and hold to repeat.', 'stepper'],
    ['Wash cycles', '1 - 10 cycles per protocol step', 'stepper2'],
    ['Auto export results', 'Write a CSV report when a run completes', 'toggleOn'],
    ['Audible alerts', 'Beep on run completion and on error', 'toggleOff'],
  ];
  for (let i = 0; i < rows.length; i++) {
    const [label, caption, kind] = rows[i];
    const y = 24 + i * 72;
    place(cardNode, await text(label, 'Body'), 24, y + 8);
    place(cardNode, await text(caption, 'Caption', 'color/text/secondary'), 24, y + 30);
    let ctl;
    if (kind === 'segmented') ctl = K.segmented.createInstance();
    else if (kind === 'stepper') ctl = variantOf(K.stepper, 'State', 'Default');
    else if (kind === 'stepper2') { ctl = variantOf(K.stepper, 'State', 'Default'); const ts = ctl.findAll(n => n.type === 'TEXT'); if (ts[1]) ts[1].characters = '3'; if (ts[2]) ts[2].characters = 'x'; }
    else ctl = variantOf(K.toggle, 'State', kind === 'toggleOn' ? 'On' : 'Off');
    place(cardNode, ctl, 1696 - 24 - ctl.width, y);
  }

  place(f, await text('Surface + Accent', 'Title'), 112, 700);
  const swatches = [['Surface.Base', 'color/surface/base'], ['Surface.Raised', 'color/surface/raised'], ['Surface.Sunken', 'color/surface/sunken'], ['Accent.Default', 'color/accent/default']];
  for (let i = 0; i < swatches.length; i++) {
    const s = frame(swatches[i][0], 412, 88, { fill: swatches[i][1], radius: 'radius/m', stroke: i === 3 ? null : 'color/border/subtle' });
    place(f, s, 112 + i * 428, 740);
    const t = await text(swatches[i][0], 'Caption', i === 3 ? 'color/text/on-accent' : 'color/text/secondary', { align: 'CENTER' });
    t.resize(412, 16);
    place(s, t, 0, 36);
  }
  return f;
}

async function logScreen(K) {
  const f = await shell('Log', K, 3);
  await header(f, 'Event Log', '5,000 entries. Toggle virtualization and watch the realized container count.');

  const seg = await segmented(['All', 'Info', 'Warning', 'Error'], 0);
  place(f, seg, 112, 176);
  const search = rect(260, 40, 'color/surface/base', { radius: 'radius/m', stroke: 'color/border/strong', name: 'Search' });
  place(f, search, 112 + seg.width + 12, 180);
  const clear = variantOf(K.secondary, 'State', 'Default');
  clear.resize(120, 40);
  const ct = clear.findOne(n => n.type === 'TEXT'); if (ct) ct.characters = 'Clear';
  place(f, clear, 112 + seg.width + 12 + 272, 180);
  place(f, await text('5000 of 5000 entries', 'Caption', 'color/text/secondary'), 1640, 190);

  const metrics = await card(1696, 72, 'Metrics');
  place(f, metrics, 112, 240);
  place(metrics, variantOf(K.toggle, 'State', 'On'), 20, 12);
  place(metrics, await text('UI virtualization', 'Body'), 84, 16);
  place(metrics, await text('VirtualizingStackPanel vs StackPanel', 'Caption', 'color/text/secondary'), 84, 40);
  const nums = [['27', 'realized containers'], ['4.2 ms', 'filter to first frame'], ['186.3 MB', 'private memory']];
  for (let i = 0; i < nums.length; i++) {
    const x = 1180 + i * 180;
    place(metrics, await text(nums[i][0], 'Title'), x, 12);
    place(metrics, await text(nums[i][1], 'Caption', 'color/text/secondary'), x, 40);
  }

  const list = frame('List', 1696, 720, { fill: 'color/surface/base', radius: 'radius/m', stroke: 'color/border/subtle', clip: true });
  place(f, list, 112, 328);
  const levels = ['Info', 'Info', 'Warning', 'Info', 'Error', 'Info', 'Info', 'Warning', 'Info', 'Info', 'Info', 'Error', 'Info', 'Warning', 'Info', 'Info', 'Info', 'Info'];
  const msgs = ['Temperature stable at 37.0 C', 'Wash cycle 2 of 3 completed', 'Temperature drift 0.4 C, compensating', 'Plate barcode scanned', 'Incubator heater timeout', 'Image captured for well C7', 'Protocol step advanced', 'Reader lamp at 82% intensity', 'Reagent volume check passed', 'Self-test completed', 'Door closed', 'Wash pump stalled', 'Image captured for well A3', 'Well F9 signal near threshold', 'Protocol step advanced', 'Temperature stable at 37.0 C', 'Wash cycle 3 of 3 completed', 'Plate barcode scanned'];
  const sources = ['Incubator', 'Washer', 'Incubator', 'PlateLoader', 'Incubator', 'Reader', 'System', 'Reader', 'System', 'System', 'System', 'Washer', 'Reader', 'Reader', 'System', 'Incubator', 'Washer', 'PlateLoader'];
  for (let i = 0; i < levels.length; i++) {
    const y = i * 40;
    place(list, rect(1696, 1, 'color/border/subtle'), 0, y + 39);
    const hh = 14 - Math.floor(i / 3); const mm = 52 - i * 2;
    place(list, await text(`${String(hh).padStart(2, '0')}:${String(mm).padStart(2, '0')}:17`, 'Caption', 'color/text/secondary'), 16, y + 12);
    place(list, variantOf(K.pill, 'Level', levels[i]), 106, y + 10);
    place(list, await text(sources[i], 'Caption', 'color/text/secondary'), 196, y + 12);
    place(list, await text(msgs[i], 'Body'), 316, y + 10);
  }
  return f;
}

// ---------- main ----------

async function main() {
  await resolveFonts();
  await createVariables();
  createTextStyles();

  const dsPage = figma.createPage();
  dsPage.name = 'Design System';
  await figma.setCurrentPageAsync(dsPage);

  const K = {};
  let y = 0;
  K.primary = await buildButtonSet('Primary', 'Start Run', dsPage, y); y += 160;
  K.secondary = await buildButtonSet('Secondary', 'Load Plate', dsPage, y); y += 160;
  K.toggle = await buildToggleSet(dsPage, y); y += 160;
  K.segmented = await buildSegmented(dsPage, y); y += 140;
  K.stepper = await buildStepperSet(dsPage, y); y += 160;
  K.dot = await buildStatusDotSet(dsPage, y); y += 120;
  K.nav = await buildNavItemSet(dsPage, y); y += 180;
  K.pill = await buildLevelPillSet(dsPage, y);

  const screensPage = figma.createPage();
  screensPage.name = 'Screens';
  await figma.setCurrentPageAsync(screensPage);

  const builders = [dashboardScreen, plateScreen, settingsScreen, logScreen];
  for (let i = 0; i < builders.length; i++) {
    const light = await builders[i](K);
    screensPage.appendChild(light);
    light.x = 0;
    light.y = i * 1200;
    light.name = `${light.name} / Light`;

    const dark = light.clone();
    dark.name = light.name.replace('/ Light', '/ Dark');
    dark.x = 2100;
    dark.y = i * 1200;
    rebindToDark(dark);
    const themeLabel = dark.findOne(n => n.type === 'TEXT' && n.characters === 'Light');
    if (themeLabel) themeLabel.characters = 'Dark';
  }

  figma.viewport.scrollAndZoomIntoView(screensPage.children);
  figma.closePlugin('MultiPlex HMI design system generated: 3 variable collections, 4 text styles, 8 components, 4 screens x Light/Dark.');
}

main().catch(err => {
  console.error(err);
  figma.closePlugin(`Failed: ${err.message}`);
});
