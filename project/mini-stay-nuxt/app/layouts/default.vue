<template>
  <div class="site">
    <a
      href="#main"
      class="skip-link"
    >跳到主要內容</a>
    <AppHeader />
    <main
      id="main"
      class="site__main"
    >
      <slot />
    </main>
    <AppFooter />
  </div>
</template>

<style lang="scss" scoped>
.site {
  display: flex;
  flex-direction: column;
  min-height: 100dvh;
}

.site__main {
  flex: 1 0 auto;
}
</style>
