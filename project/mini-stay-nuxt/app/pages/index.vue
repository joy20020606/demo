<script setup lang="ts">
useSeoMeta({ title: '首頁' })
</script>

<template>
  <div>
    <section class="hero-placeholder">
      <div class="container hero-placeholder__inner">
        <p class="eyebrow">
          Best place to relax &amp; enjoy
        </p>
        <h1 class="hero-placeholder__title">
          Perfect Place to<br>Enjoy Your Life
        </h1>
        <p class="hero-placeholder__note">
          Phase 1 骨架:此區塊在 Phase 2 換成 Figma hero 還原。
        </p>
      </div>
    </section>
  </div>
</template>

<style lang="scss" scoped>
.hero-placeholder {
  min-height: 100dvh;
  display: flex;
  align-items: center;
  padding-block: calc(var(--header-h) + var(--space-8)) var(--space-10);
  background: linear-gradient(180deg, #4a4340 0%, var(--color-grey-500) 100%);
  color: var(--color-white);
}

.hero-placeholder__inner {
  display: flex;
  flex-direction: column;
  gap: var(--space-5);
}

.hero-placeholder__note {
  color: var(--color-grey-100);
  font-size: var(--fs-small);
}
</style>
