<script setup lang="ts">
withDefaults(defineProps<{ size?: 'sm' | 'lg' }>(), { size: 'sm' })
</script>

<template>
  <NuxtLink
    to="/"
    class="brand"
    :class="`brand--${size}`"
    aria-label="Mini Stay 首頁"
  >
    <img
      :src="size === 'lg' ? '/images/brand/logo-mark-lg.svg' : '/images/brand/logo-mark.svg'"
      alt=""
      class="brand__mark"
      :width="size === 'lg' ? 110 : 69"
      :height="size === 'lg' ? 76 : 48"
    >
    <span class="brand__text">
      <span class="brand__name">Mini Stay</span>
      <span class="brand__tagline">Boutique Homestay</span>
    </span>
  </NuxtLink>
</template>

<style lang="scss" scoped>
.brand {
  display: inline-flex;
  align-items: center;
  gap: var(--space-3);
  color: var(--color-white);
  text-transform: uppercase;
  line-height: 1;
}

.brand__mark {
  flex: 0 0 auto;
  width: 45px;
  height: auto;

  @include up(lg) {
    width: 69px;
  }
}

.brand__text {
  display: flex;
  flex-direction: column;
  gap: 2px;
  white-space: nowrap;
}

.brand__name {
  font-family: var(--font-serif);
  font-size: 20px;
  letter-spacing: 0.04em;

  @include up(lg) {
    font-size: 36px;
  }
}

.brand__tagline {
  font-family: var(--font-sans);
  font-weight: 400;
  font-size: 7px;
  letter-spacing: 0.55em;

  @include up(lg) {
    font-size: 12px;
  }
}

.brand--lg {
  gap: var(--space-5);

  .brand__mark {
    width: 80px;

    @include up(lg) {
      width: 110px;
    }
  }

  .brand__name {
    font-size: 36px;

    @include up(lg) {
      font-size: 54px;
    }
  }

  .brand__tagline {
    font-size: 11px;

    @include up(lg) {
      font-size: 18px;
    }
  }
}
</style>
