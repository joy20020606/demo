<script setup lang="ts">
useSeoMeta({ title: '房型' })
</script>

<template>
  <section class="rooms-placeholder">
    <div class="container">
      <p class="eyebrow">
        Rooms
      </p>
      <h1>房型列表</h1>
      <p>Phase 3 實作:列表、篩選與 SSR 資料。</p>
    </div>
  </section>
</template>

<style lang="scss" scoped>
.rooms-placeholder {
  padding-block: calc(var(--header-h) + var(--space-8)) var(--space-10);
  background: var(--color-grey-500);
  color: var(--color-white);
  min-height: 60dvh;
}
</style>
