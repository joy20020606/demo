/* TOEIC 3000 單字練習 — 應用邏輯 */
(function () {
  'use strict';

  var WORDS = (window.WORDS || []).map(function (w, i) {
    return { id: i, en: w[0], pos: w[1], zh: w[2], lv: w[3] };
  });

  // ---------- 進度儲存 ----------
  var STORE_KEY = 'toeic3000.progress.v1';
  var progress = load();

  function load() {
    try {
      var raw = localStorage.getItem(STORE_KEY);
      if (raw) return JSON.parse(raw);
    } catch (e) { /* 無痕模式或封鎖儲存 */ }
    return { words: {}, sessions: 0, totalRight: 0, totalWrong: 0 };
  }
  function save() {
    try { localStorage.setItem(STORE_KEY, JSON.stringify(progress)); } catch (e) {}
  }
  function rec(en) {
    if (!progress.words[en]) progress.words[en] = { r: 0, w: 0, streak: 0, star: 0, seen: 0 };
    return progress.words[en];
  }
  function mastered(en) {
    var p = progress.words[en];
    return !!p && p.streak >= 3;
  }

  // ---------- 小工具 ----------
  var $ = function (id) { return document.getElementById(id); };
  function shuffle(a) {
    for (var i = a.length - 1; i > 0; i--) {
      var j = Math.floor(Math.random() * (i + 1));
      var t = a[i]; a[i] = a[j]; a[j] = t;
    }
    return a;
  }
  function show(sec) {
    ['home', 'quiz', 'result', 'list', 'stats'].forEach(function (s) {
      $(s).classList.toggle('hidden', s !== sec);
    });
    window.scrollTo(0, 0);
  }
  // 中文字義去掉詞性註記，取第一個義項當比對用
  function zhMain(zh) { return zh.split(/[；;]/)[0].trim(); }

  // ---------- 設定 ----------
  var cfg = { mode: 'en2zh', levels: { 1: 1, 2: 1, 3: 1 }, pos: {}, pool: 'all', n: 20, smart: true, autoSpeak: false };

  // 詞性選項（依單字庫實際出現的詞性動態產生）
  var POS_LABEL = { v: '動詞', n: '名詞', adj: '形容詞', adv: '副詞', prep: '介系詞', conj: '連接詞', phr: '片語', pron: '代名詞' };
  var POS_ORDER = ['n', 'v', 'adj', 'adv', 'prep', 'conj', 'pron', 'phr'];
  var posList = [];
  (function () {
    var seen = {};
    WORDS.forEach(function (w) { if (!seen[w.pos]) { seen[w.pos] = 1; posList.push(w.pos); } });
    posList.sort(function (a, b) {
      var ia = POS_ORDER.indexOf(a), ib = POS_ORDER.indexOf(b);
      return (ia < 0 ? 99 : ia) - (ib < 0 ? 99 : ib) || (a < b ? -1 : 1);
    });
    var box = $('posChips');
    box.innerHTML = '<div class="chip on" data-pos="all">全部詞性</div>' + posList.map(function (p) {
      return '<div class="chip" data-pos="' + p + '">' + (POS_LABEL[p] || p) + '</div>';
    }).join('');
  })();

  // ---------- 題庫篩選 ----------
  function pool() {
    var posAll = $('posChips').querySelector('[data-pos="all"]').classList.contains('on');
    return WORDS.filter(function (w) {
      if (!cfg.levels[w.lv]) return false;
      if (!posAll && !cfg.pos[w.pos]) return false;
      var p = progress.words[w.en];
      if (cfg.pool === 'wrong') return !!p && p.w > 0;
      if (cfg.pool === 'star') return !!p && p.star;
      if (cfg.pool === 'new') return !p || !p.seen;
      return true;
    });
  }
  function updatePoolInfo() {
    var n = pool().length;
    $('poolInfo').textContent = '符合條件：' + n + ' 字';
    $('startBtn').disabled = n < 4;
    if (n > 0 && n < 4) $('poolInfo').textContent = '符合條件僅 ' + n + ' 字，選擇題至少需要 4 字';
  }

  // 智慧排序：錯越多、越沒練過的權重越高
  function weightOf(w) {
    var p = progress.words[w.en];
    if (!p) return 3;                        // 沒練過
    if (p.streak >= 3) return 0.35;          // 已精熟，偶爾複習
    return 1 + p.w * 2 - Math.min(p.streak, 2) * 0.3;
  }
  function pickQuestions() {
    var list = pool().slice();
    if (cfg.smart) {
      list.forEach(function (w) { w._k = Math.random() / Math.max(weightOf(w), 0.05); });
      list.sort(function (a, b) { return a._k - b._k; });
    } else {
      shuffle(list);
    }
    var n = cfg.n === 0 ? list.length : Math.min(cfg.n, list.length);
    return list.slice(0, n);
  }

  // 干擾選項：優先挑同詞性、同難度的字，並排除字義／拼字相同者
  function distractors(ans, key) {
    var norm = key === 'zh'
      ? function (w) { return zhMain(w.zh); }
      : function (w) { return w.en.toLowerCase(); };
    var akey = norm(ans);
    var usable = function (w) { return w.id !== ans.id && norm(w) !== akey; };
    var same = WORDS.filter(function (w) { return usable(w) && w.pos === ans.pos; });
    var prefer = same.filter(function (w) { return w.lv === ans.lv; });
    var src = prefer.length >= 8 ? prefer : (same.length >= 8 ? same : WORDS.filter(usable));
    var picked = [], used = {};
    used[akey] = 1;
    var guard = 0;
    while (picked.length < 3 && guard++ < 500) {
      var c = src[Math.floor(Math.random() * src.length)];
      if (!c || used[norm(c)]) continue;
      used[norm(c)] = 1;
      picked.push(c);
    }
    return picked;
  }

  // ---------- 測驗狀態 ----------
  var S = { qs: [], i: 0, right: 0, wrong: 0, wrongList: [], answered: false, opts: [], mode: 'en2zh' };

  function startQuiz(list) {
    S.qs = list || pickQuestions();
    if (!S.qs.length) return;
    S.i = 0; S.right = 0; S.wrong = 0; S.wrongList = []; S.mode = cfg.mode;
    progress.sessions++;
    save();
    show('quiz');
    renderQ();
  }

  function renderQ() {
    S.answered = false;
    var q = S.qs[S.i];
    $('qProgress').textContent = (S.i + 1) + ' / ' + S.qs.length;
    $('qLevel').textContent = 'L' + q.lv;
    $('qScore').textContent = '✅ ' + S.right + '　❌ ' + S.wrong;
    $('qBar').style.width = (S.i / S.qs.length * 100) + '%';
    $('feedback').className = 'feedback';
    $('feedback').innerHTML = '';
    $('nextBtn').classList.add('hidden');
    var p = progress.words[q.en];
    $('starBtn').classList.toggle('on', !!(p && p.star));
    $('starBtn').textContent = (p && p.star) ? '★' : '☆';

    var body = $('qBody');
    if (S.mode === 'en2zh' || S.mode === 'zh2en') {
      var isEn = S.mode === 'en2zh';
      var key = isEn ? 'zh' : 'en';
      S.opts = shuffle(distractors(q, key).concat([q]));
      body.innerHTML =
        '<div class="prompt' + (isEn ? '' : ' zh') + '">' + esc(isEn ? q.en : q.zh) + '</div>' +
        '<div class="pos">' + esc(isEn ? (POS_LABEL[q.pos] || q.pos) : '請選出正確的英文單字') + '</div>' +
        '<div class="options">' + S.opts.map(function (o, k) {
          return '<button class="opt" data-k="' + k + '"><span class="k">' + (k + 1) + '</span>' + esc(o[key]) + '</button>';
        }).join('') + '</div>';
      [].forEach.call(body.querySelectorAll('.opt'), function (b) {
        b.onclick = function () { answerChoice(+b.dataset.k); };
      });
      $('kbHint').textContent = '快捷鍵：1–4 選答案，Enter 下一題';
    } else if (S.mode === 'spell') {
      body.innerHTML =
        '<div class="prompt zh">' + esc(q.zh) + '</div>' +
        '<div class="pos">' + esc(POS_LABEL[q.pos] || q.pos) + '　·　' +
        esc(hintOf(q.en)) + '（' + q.en.length + ' 個字母）</div>' +
        '<input type="text" id="spellInput" autocomplete="off" autocapitalize="off" spellcheck="false" placeholder="輸入英文單字後按 Enter">' +
        '<div class="row" style="margin-top:12px"><button class="primary" id="checkBtn">送出</button>' +
        '<button class="ghost" id="giveUpBtn">看答案</button></div>';
      $('checkBtn').onclick = checkSpell;
      $('giveUpBtn').onclick = function () { finishSpell(false, true, null); };
      setTimeout(function () { $('spellInput').focus(); }, 30);
      $('kbHint').textContent = '快捷鍵：Enter 送出／下一題';
    } else { // flash
      body.innerHTML =
        '<div class="flash" id="flashCard">' +
        '<div class="prompt">' + esc(q.en) + '</div>' +
        '<div class="pos">' + esc(POS_LABEL[q.pos] || q.pos) + '</div>' +
        '<div class="hint">點一下卡片看中文字義（或按空白鍵）</div></div>';
      $('flashCard').onclick = flip;
      $('kbHint').textContent = '快捷鍵：空白鍵翻卡，1 = 不熟，2 = 記得';
    }
    speak(q.en, true);
  }

  function hintOf(en) {
    // 提示：首字母 + 末字母，其餘以底線表示（保留空白與連字號）
    return en.split('').map(function (c, i) {
      if (c === ' ' || c === '-') return c;
      return (i === 0 || i === en.length - 1) ? c : '_';
    }).join('');
  }

  function flip() {
    if (S.answered) return;
    S.answered = true;
    var q = S.qs[S.i];
    $('flashCard').innerHTML =
      '<div class="prompt zh">' + esc(q.zh) + '</div>' +
      '<div class="pos">' + esc(q.en) + '</div>';
    $('flashCard').onclick = null;
    var fb = $('feedback');
    fb.className = 'feedback show';
    fb.innerHTML = '<div class="row"><b style="align-self:center">這個字你記得嗎？</b><span class="spacer"></span>' +
      '<button id="fNo">1 · 還不熟</button><button class="primary" id="fYes">2 · 記得</button></div>';
    $('fNo').onclick = function () { gradeFlash(false); };
    $('fYes').onclick = function () { gradeFlash(true); };
  }
  function gradeFlash(ok) {
    var q = S.qs[S.i];
    mark(q, ok);
    $('feedback').className = 'feedback show ' + (ok ? 'ok' : 'no');
    $('feedback').innerHTML = ok ? '✅ 記得了，繼續保持' : '❌ 已加入待加強清單';
    $('nextBtn').classList.remove('hidden');
    $('nextBtn').focus();
  }

  function answerChoice(k) {
    if (S.answered) return;
    S.answered = true;
    var q = S.qs[S.i];
    var ok = S.opts[k].id === q.id;
    var btns = $('qBody').querySelectorAll('.opt');
    [].forEach.call(btns, function (b, idx) {
      b.disabled = true;
      if (S.opts[idx].id === q.id) b.classList.add('correct');
      else if (idx === k) b.classList.add('wrong');
    });
    mark(q, ok);
    var fb = $('feedback');
    fb.className = 'feedback show ' + (ok ? 'ok' : 'no');
    fb.innerHTML = (ok ? '✅ 答對了！' : '❌ 答錯了') +
      '<div style="margin-top:6px"><b>' + esc(q.en) + '</b>　<span class="muted">' +
      esc(POS_LABEL[q.pos] || q.pos) + '</span><br>' + esc(q.zh) + '</div>';
    $('nextBtn').classList.remove('hidden');
    if (!ok) speak(q.en);
  }

  function checkSpell() {
    if (S.answered) return;
    var q = S.qs[S.i];
    var val = ($('spellInput').value || '').trim().toLowerCase();
    if (!val) return;
    if (val === q.en.toLowerCase()) return finishSpell(true, false, null);
    // 同義字也算對：單字庫中有 209 組中文字義完全相同，
    // 例如「精確的」對應 accurate / exact / precise，不該把使用者的正解判為錯。
    var syn = WORDS.filter(function (w) {
      return w.id !== q.id && w.en.toLowerCase() === val && zhMain(w.zh) === zhMain(q.zh);
    })[0];
    finishSpell(!!syn, false, syn);
  }
  function finishSpell(ok, gaveUp, syn) {
    S.answered = true;
    var q = S.qs[S.i];
    mark(q, ok);
    $('spellInput').disabled = true;
    $('checkBtn').disabled = true;
    $('giveUpBtn').disabled = true;
    var fb = $('feedback');
    fb.className = 'feedback show ' + (ok ? 'ok' : 'no');
    var head = syn ? '✅ 同義字也算對！'
      : (ok ? '✅ 拼對了！' : (gaveUp ? '正確答案是' : '❌ 拼錯了，正確答案是'));
    fb.innerHTML = head +
      '<div style="margin-top:6px"><b>' + esc(q.en) + '</b>　<span class="muted">' + esc(q.zh) + '</span></div>' +
      (syn ? '<div class="muted" style="margin-top:4px">你寫的 ' + esc(syn.en) + ' 也是這個意思</div>' : '');
    $('nextBtn').classList.remove('hidden');
    if (!ok) speak(q.en);
  }

  function mark(q, ok) {
    var p = rec(q.en);
    p.seen++;
    if (ok) { p.r++; p.streak++; progress.totalRight++; S.right++; }
    else { p.w++; p.streak = 0; progress.totalWrong++; S.wrong++; S.wrongList.push(q); }
    $('qScore').textContent = '✅ ' + S.right + '　❌ ' + S.wrong;
    $('qBar').style.width = ((S.i + 1) / S.qs.length * 100) + '%';
    save();
  }

  function next() {
    S.i++;
    if (S.i >= S.qs.length) return finish();
    renderQ();
  }

  function finish() {
    var total = S.right + S.wrong;
    var pct = total ? Math.round(S.right / total * 100) : 0;
    $('rScore').textContent = pct + '%';
    $('rScore').style.color = pct >= 80 ? 'var(--ok)' : (pct >= 60 ? 'var(--warn)' : 'var(--bad)');
    $('rDetail').textContent = '共 ' + total + ' 題　✅ 答對 ' + S.right + '　❌ 答錯 ' + S.wrong;
    $('rWrongCount').textContent = S.wrongList.length;
    $('reviewWrongBtn').classList.toggle('hidden', S.wrongList.length === 0);
    $('rWrongCard').classList.toggle('hidden', S.wrongList.length === 0);
    $('rWrongList').innerHTML = '<table><tbody>' + S.wrongList.map(function (w) {
      return '<tr><td style="width:38%"><b>' + esc(w.en) + '</b></td><td class="muted">' +
        esc(POS_LABEL[w.pos] || w.pos) + '</td><td>' + esc(w.zh) + '</td></tr>';
    }).join('') + '</tbody></table>';
    show('result');
    renderHome();
  }

  function esc(s) {
    return String(s).replace(/[&<>"]/g, function (c) {
      return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c];
    });
  }

  // ---------- 發音 ----------
  var voice = null, voiceReady = false;
  function pickVoice() {
    if (!('speechSynthesis' in window)) return;
    var vs = speechSynthesis.getVoices();
    voice = vs.filter(function (v) { return /^en(-|_)/i.test(v.lang); })[0] || null;
    voiceReady = vs.length > 0;
  }
  if ('speechSynthesis' in window) {
    pickVoice();
    speechSynthesis.onvoiceschanged = pickVoice;
  }
  function speak(text, auto) {
    if (!('speechSynthesis' in window)) return;
    if (auto && !cfg.autoSpeak) return;
    if (!voiceReady) pickVoice();
    try {
      speechSynthesis.cancel();
      var u = new SpeechSynthesisUtterance(text);
      u.lang = 'en-US'; u.rate = 0.9;
      if (voice) u.voice = voice;
      speechSynthesis.speak(u);
    } catch (e) {}
  }

  // ---------- 首頁 / 統計 ----------
  function counts() {
    var seen = 0, mast = 0, weak = 0, star = 0;
    Object.keys(progress.words).forEach(function (k) {
      var p = progress.words[k];
      if (p.seen) seen++;
      if (p.streak >= 3) mast++;
      if (p.w > 0 && p.streak < 3) weak++;
      if (p.star) star++;
    });
    return { seen: seen, mast: mast, weak: weak, star: star };
  }
  function renderHome() {
    var c = counts();
    var acc = (progress.totalRight + progress.totalWrong)
      ? Math.round(progress.totalRight / (progress.totalRight + progress.totalWrong) * 100) : 0;
    $('homeStats').innerHTML = [
      ['已練過', c.seen], ['已精熟', c.mast], ['待加強', c.weak], ['正確率', acc + '%']
    ].map(function (s) {
      return '<div class="stat"><div class="n">' + s[1] + '</div><div class="l">' + s[0] + '</div></div>';
    }).join('');
    var pct = WORDS.length ? Math.round(c.mast / WORDS.length * 100) : 0;
    $('masterBar').style.width = pct + '%';
    $('masterPct').textContent = c.mast + ' / ' + WORDS.length + '（' + pct + '%）';
    updatePoolInfo();
  }
  function renderStats() {
    var c = counts();
    var acc = (progress.totalRight + progress.totalWrong)
      ? Math.round(progress.totalRight / (progress.totalRight + progress.totalWrong) * 100) : 0;
    $('statGrid').innerHTML = [
      ['單字總數', WORDS.length], ['已練過', c.seen], ['已精熟', c.mast],
      ['待加強', c.weak], ['收藏', c.star], ['練習場次', progress.sessions],
      ['累計答對', progress.totalRight], ['累計答錯', progress.totalWrong], ['整體正確率', acc + '%']
    ].map(function (s) {
      return '<div class="stat"><div class="n">' + s[1] + '</div><div class="l">' + s[0] + '</div></div>';
    }).join('');

    var weak = WORDS.filter(function (w) {
      var p = progress.words[w.en];
      return p && p.w > 0;
    }).sort(function (a, b) {
      var pa = progress.words[a.en], pb = progress.words[b.en];
      return (pb.w - pb.r) - (pa.w - pa.r) || pb.w - pa.w;
    }).slice(0, 30);

    $('weakBody').innerHTML = weak.map(function (w) {
      var p = progress.words[w.en];
      return '<tr><td style="width:32%"><b>' + esc(w.en) + '</b></td><td>' + esc(w.zh) +
        '</td><td style="width:22%" class="muted">✅' + p.r + ' ❌' + p.w +
        (mastered(w.en) ? ' <span class="tag ok">精熟</span>' : '') + '</td></tr>';
    }).join('');
    $('weakEmpty').textContent = weak.length ? '' : '目前還沒有答錯紀錄，先去練幾輪吧！';
  }

  // ---------- 單字表 ----------
  var listFilter = 'all', listShown = 100;
  function renderList() {
    var q = ($('listSearch').value || '').trim().toLowerCase();
    var rows = WORDS.filter(function (w) {
      if (q && w.en.toLowerCase().indexOf(q) < 0 && w.zh.indexOf(q) < 0) return false;
      var p = progress.words[w.en];
      if (listFilter === 'star') return !!(p && p.star);
      if (listFilter === 'wrong') return !!(p && p.w > 0);
      if (listFilter === 'mastered') return mastered(w.en);
      if (listFilter !== 'all') return w.lv === +listFilter;
      return true;
    });
    $('listInfo').textContent = '共 ' + rows.length + ' 字' + (rows.length > listShown ? '（顯示前 ' + listShown + ' 筆）' : '');
    $('listBody').innerHTML = rows.slice(0, listShown).map(function (w) {
      var p = progress.words[w.en] || { r: 0, w: 0, star: 0 };
      return '<tr>' +
        '<td style="width:30%"><b>' + esc(w.en) + '</b><br><span class="muted">' + esc(POS_LABEL[w.pos] || w.pos) + '</span></td>' +
        '<td>' + esc(w.zh) + '</td>' +
        '<td style="width:20%"><span class="tag">L' + w.lv + '</span> ' +
        (mastered(w.en) ? '<span class="tag ok">精熟</span>' : (p.w > 0 ? '<span class="tag bad">❌' + p.w + '</span>' : '')) + '</td>' +
        '<td style="width:44px"><button class="star ' + (p.star ? 'on' : '') + '" data-en="' + esc(w.en) + '">' +
        (p.star ? '★' : '☆') + '</button></td></tr>';
    }).join('');
    [].forEach.call($('listBody').querySelectorAll('.star'), function (b) {
      b.onclick = function () {
        var p = rec(b.dataset.en);
        p.star = p.star ? 0 : 1;
        save(); renderList();
      };
    });
    $('moreBtn').classList.toggle('hidden', rows.length <= listShown);
  }

  // ---------- 事件綁定 ----------
  function chipGroup(boxId, onPick) {
    $(boxId).addEventListener('click', function (e) {
      var c = e.target.closest('.chip');
      if (c) onPick(c, this);
    });
  }
  chipGroup('levelChips', function (c) {
    var lv = +c.dataset.lv;
    var on = c.classList.toggle('on');
    cfg.levels[lv] = on ? 1 : 0;
    if (!cfg.levels[1] && !cfg.levels[2] && !cfg.levels[3]) { c.classList.add('on'); cfg.levels[lv] = 1; }
    updatePoolInfo();
  });
  chipGroup('posChips', function (c, box) {
    var v = c.dataset.pos;
    var all = box.querySelector('[data-pos="all"]');
    if (v === 'all') {
      all.classList.add('on');
      [].forEach.call(box.querySelectorAll('.chip'), function (x) { if (x !== all) x.classList.remove('on'); });
      cfg.pos = {};
    } else {
      c.classList.toggle('on');
      cfg.pos[v] = c.classList.contains('on') ? 1 : 0;
      var any = Object.keys(cfg.pos).some(function (k) { return cfg.pos[k]; });
      all.classList.toggle('on', !any);
    }
    updatePoolInfo();
  });
  chipGroup('poolChips', function (c, box) {
    [].forEach.call(box.querySelectorAll('.chip'), function (x) { x.classList.remove('on'); });
    c.classList.add('on');
    cfg.pool = c.dataset.pool;
    updatePoolInfo();
  });
  chipGroup('countChips', function (c, box) {
    [].forEach.call(box.querySelectorAll('.chip'), function (x) { x.classList.remove('on'); });
    c.classList.add('on');
    cfg.n = +c.dataset.n;
  });
  $('modeGrid').addEventListener('click', function (e) {
    var m = e.target.closest('.mode');
    if (!m) return;
    [].forEach.call(this.querySelectorAll('.mode'), function (x) { x.classList.remove('on'); });
    m.classList.add('on');
    cfg.mode = m.dataset.mode;
  });
  $('smartOrder').onchange = function () { cfg.smart = this.checked; };
  $('autoSpeak').onchange = function () { cfg.autoSpeak = this.checked; };

  $('startBtn').onclick = function () { startQuiz(); };
  $('nextBtn').onclick = next;
  $('quitBtn').onclick = function () {
    if (S.right + S.wrong > 0) finish(); else { show('home'); renderHome(); }
  };
  $('againBtn').onclick = function () { startQuiz(); };
  $('reviewWrongBtn').onclick = function () { startQuiz(shuffle(S.wrongList.slice())); };
  $('backHomeBtn').onclick = function () { show('home'); renderHome(); };
  $('starBtn').onclick = function () {
    var q = S.qs[S.i];
    var p = rec(q.en);
    p.star = p.star ? 0 : 1;
    save();
    $('starBtn').classList.toggle('on', !!p.star);
    $('starBtn').textContent = p.star ? '★' : '☆';
  };
  $('speakBtn').onclick = function () { speak(S.qs[S.i].en); };

  $('navHome').onclick = function () { show('home'); renderHome(); };
  $('navList').onclick = function () { show('list'); listShown = 100; renderList(); };
  $('navStats').onclick = function () { show('stats'); renderStats(); };
  $('listSearch').oninput = function () { listShown = 100; renderList(); };
  $('moreBtn').onclick = function () { listShown += 200; renderList(); };
  $('listTabs').addEventListener('click', function (e) {
    var b = e.target.closest('button');
    if (!b) return;
    [].forEach.call(this.querySelectorAll('button'), function (x) { x.classList.remove('on'); });
    b.classList.add('on');
    listFilter = b.dataset.f;
    listShown = 100;
    renderList();
  });

  // 主題切換
  var savedTheme = null;
  try { savedTheme = localStorage.getItem('toeic3000.theme'); } catch (e) {}
  if (savedTheme) document.documentElement.dataset.theme = savedTheme;
  $('themeBtn').onclick = function () {
    var cur = document.documentElement.dataset.theme;
    var isDark = cur ? cur === 'dark'
      : window.matchMedia('(prefers-color-scheme: dark)').matches;
    var next = isDark ? 'light' : 'dark';
    document.documentElement.dataset.theme = next;
    try { localStorage.setItem('toeic3000.theme', next); } catch (e) {}
  };

  // 資料管理
  $('exportBtn').onclick = function () {
    var blob = new Blob([JSON.stringify(progress, null, 2)], { type: 'application/json' });
    var a = document.createElement('a');
    a.href = URL.createObjectURL(blob);
    a.download = 'toeic3000-progress.json';
    a.click();
    setTimeout(function () { URL.revokeObjectURL(a.href); }, 1000);
  };
  $('importBtn').onclick = function () { $('importFile').click(); };
  $('importFile').onchange = function () {
    var f = this.files[0];
    if (!f) return;
    var r = new FileReader();
    r.onload = function () {
      try {
        var d = JSON.parse(r.result);
        if (!d || typeof d.words !== 'object') throw new Error('格式錯誤');
        progress = {
          words: d.words || {}, sessions: d.sessions || 0,
          totalRight: d.totalRight || 0, totalWrong: d.totalWrong || 0
        };
        save(); renderStats(); renderHome();
        alert('匯入成功');
      } catch (e) { alert('匯入失敗：檔案格式不正確'); }
    };
    r.readAsText(f);
    this.value = '';
  };
  $('resetBtn').onclick = function () {
    if (!confirm('確定要清除所有學習紀錄嗎？此操作無法復原。')) return;
    progress = { words: {}, sessions: 0, totalRight: 0, totalWrong: 0 };
    save(); renderStats(); renderHome();
  };

  // 鍵盤快捷鍵
  document.addEventListener('keydown', function (e) {
    if ($('quiz').classList.contains('hidden')) return;
    var typing = e.target.tagName === 'INPUT';
    if (e.key === 'Enter') {
      e.preventDefault();
      if (S.answered) return next();
      if (S.mode === 'spell') return checkSpell();
      return;
    }
    if (typing) return;
    if (S.mode === 'flash') {
      if (e.key === ' ') { e.preventDefault(); if (!S.answered) flip(); }
      if (S.answered && (e.key === '1' || e.key === '2')) gradeFlash(e.key === '2');
      return;
    }
    if (!S.answered && /^[1-4]$/.test(e.key) && S.opts.length) answerChoice(+e.key - 1);
  });

  // ---------- 初始化 ----------
  $('wordCount').textContent = '· 收錄 ' + WORDS.length + ' 字';
  renderHome();
})();
