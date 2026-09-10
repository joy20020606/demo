<script setup lang="ts">
const links = [
  { label: 'Home', to: '/' },
  { label: 'Rooms', to: '/rooms' },
  { label: 'About', to: '/#about' },
  { label: 'Service', to: '/#service' },
  { label: 'Contact Us', to: '/#contact' },
]

const open = ref(false)
const panelId = useId()
const route = useRoute()

// hash 連結(/#about)在首頁不算「目前頁面」,只有 path 完全相同才亮金色
function isCurrent(to: string) {
  return !to.includes('#') && route.path === to
}

watch(() => route.fullPath, () => {
  open.value = false
})

useHead({
  bodyAttrs: { class: computed(() => (open.value ? 'is-locked' : '')) },
})

function onKeydown(e: KeyboardEvent) {
  if (e.key === 'Escape') open.value = false
}

// 手機開著選單再拉寬到桌機斷點時自動關閉,避免 body 一直鎖捲動
let desktopQuery: MediaQueryList | undefined
function onBreakpointChange(e: MediaQueryListEvent) {
  if (e.matches) open.value = false
}

onMounted(() => {
  window.addEventListener('keydown', onKeydown)
  desktopQuery = window.matchMedia('(min-width: 1024px)')
  desktopQuery.addEventListener('change', onBreakpointChange)
})
onBeforeUnmount(() => {
  window.removeEventListener('keydown', onKeydown)
  desktopQuery?.removeEventListener('change', onBreakpointChange)
})
</script>

<template>
  <header
    class="header"
    :class="{ 'is-open': open }"
  >
    <div class="header__bar container">
      <BrandLogo />

      <button
        type="button"
        class="header__toggle"
        :aria-expanded="open"
        :aria-controls="panelId"
        @click="open = !open"
      >
        <span class="visually-hidden">{{ open ? '關閉選單' : '開啟選單' }}</span>
        <span
          class="header__burger"
          aria-hidden="true"
        />
      </button>

      <nav
        :id="panelId"
        class="nav"
        aria-label="主要導覽"
      >
        <ul class="nav__list">
          <li
            v-for="link in links"
            :key="link.to"
            class="nav__item"
          >
            <NuxtLink
              :to="link.to"
              class="nav__link"
              :class="{ 'is-active': isCurrent(link.to) }"
              :aria-current="isCurrent(link.to) ? 'page' : undefined"
            >
              {{ link.label }}
            </NuxtLink>
          </li>
        </ul>

        <div class="nav__social">
          <span class="nav__rule" />
          <SocialLinks size="sm" />
          <span class="nav__rule" />
        </div>
      </nav>
    </div>
  </header>
</template>

<style lang="scss" scoped>
.header {
  position: absolute;
  inset: 0 0 auto 0;
  z-index: 50;
  color: var(--color-white);
}

.header__bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  height: var(--header-h);
}

// ---- Hamburger(< lg)----
.header__toggle {
  position: relative;
  z-index: 2;
  display: grid;
  place-items: center;
  width: 44px;
  height: 44px;
  margin-inline-end: -10px;
  color: var(--color-white);

  @include up(lg) {
    display: none;
  }
}

.header__burger {
  position: relative;
  display: block;
  width: 24px;
  height: 2px;
  background: currentcolor;
  transition: background var(--dur) var(--ease);

  &::before,
  &::after {
    content: '';
    position: absolute;
    inset-inline: 0;
    height: 2px;
    background: currentcolor;
    transition: transform var(--dur) var(--ease);
  }

  &::before {
    top: -8px;
  }

  &::after {
    top: 8px;
  }
}

.is-open .header__burger {
  background: transparent;

  &::before {
    transform: translateY(8px) rotate(45deg);
  }

  &::after {
    transform: translateY(-8px) rotate(-45deg);
  }
}

// ---- Overlay panel(< lg)----
.nav {
  position: fixed;
  inset: 0;
  z-index: 0;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  gap: var(--space-10);
  padding: calc(var(--header-h) + var(--space-12)) var(--container-pad) var(--space-9);
  overflow-y: auto;
  background: var(--color-grey-500);
  opacity: 0;
  visibility: hidden;
  transform: translateY(-8px);
  transition:
    opacity var(--dur) var(--ease),
    transform var(--dur) var(--ease),
    visibility 0s linear var(--dur);

  .is-open & {
    opacity: 1;
    visibility: visible;
    transform: none;
    transition-delay: 0s;
  }
}

.is-open .header__bar::before {
  content: '';
  position: fixed;
  inset: 0 0 auto 0;
  height: var(--header-h);
  background: var(--color-grey-450);
  z-index: 1;
}

.is-open :deep(.brand) {
  position: relative;
  z-index: 2;
}

.nav__list {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-6);
  text-align: center;
}

.nav__link {
  position: relative;
  display: inline-block;
  font-size: 30px;
  font-weight: 300;
  text-transform: uppercase;
  letter-spacing: 0.02em;
  transition: color var(--dur) var(--ease);

  &:hover,
  &.is-active {
    color: var(--color-gold);
  }
}

.nav__social {
  display: flex;
  align-items: center;
  gap: var(--space-7);
}

.nav__rule {
  flex: 1 1 0;
  height: 1px;
  background: var(--color-grey-100);
}

// ---- Inline nav(≥ lg)----
@include up(lg) {
  .nav {
    position: static;
    flex-direction: row;
    padding: 0;
    background: transparent;
    opacity: 1;
    visibility: visible;
    transform: none;
    transition: none;
  }

  .nav__list {
    flex-direction: row;
    gap: var(--space-9);
  }

  .nav__link {
    font-size: var(--fs-body);
    letter-spacing: 0;
    padding-block: var(--space-1);

    &::after {
      content: '';
      position: absolute;
      left: 50%;
      bottom: -10px;
      width: 6px;
      height: 6px;
      border-radius: 50%;
      background: var(--color-gold);
      transform: translateX(-50%) scale(0);
      transition: transform var(--dur) var(--ease);
    }

    &.is-active::after {
      transform: translateX(-50%) scale(1);
    }
  }

  .nav__social {
    display: none;
  }

  // 手機開著選單再拉寬視窗時,open 仍為 true;桌機不畫頂欄底色,連結才不會被蓋住
  .is-open .header__bar::before {
    content: none;
  }
}
</style>
