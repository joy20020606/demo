<script setup lang="ts">
withDefaults(defineProps<{ size?: 'sm' | 'md' }>(), { size: 'md' })

const items = [
  { name: 'Twitter', icon: '/icons/twitter.svg' },
  { name: 'Facebook', icon: '/icons/facebook.svg' },
  { name: 'Instagram', icon: '/icons/instagram.svg' },
  { name: 'Pinterest', icon: '/icons/pinterest.svg' },
  { name: 'Dribbble', icon: '/icons/dribbble.svg' },
]
</script>

<template>
  <ul
    class="social"
    :class="`social--${size}`"
  >
    <li
      v-for="item in items"
      :key="item.name"
    >
      <a
        href="#"
        class="social__link"
        :aria-label="item.name"
        rel="noopener"
      >
        <img
          :src="item.icon"
          alt=""
          class="social__icon"
        >
      </a>
    </li>
  </ul>
</template>

<style lang="scss" scoped>
.social {
  display: flex;
  align-items: center;
  gap: var(--space-4);
}

.social--sm {
  gap: 15px;
}

.social__link {
  display: block;
  border-radius: 50%;
  transition: transform var(--dur) var(--ease);

  &:hover {
    transform: translateY(-2px);
  }
}

.social__icon {
  width: 40px;
  height: 40px;

  .social--sm & {
    width: 28px;
    height: 28px;
  }
}
</style>
