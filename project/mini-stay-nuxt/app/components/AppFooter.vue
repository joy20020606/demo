<script setup lang="ts">
const year = new Date().getFullYear()
const links = [
  { label: 'Home', to: '/' },
  { label: 'About', to: '/#about' },
  { label: 'Service', to: '/#service' },
  { label: 'Rooms', to: '/rooms' },
]
const email = ref('')
</script>

<template>
  <footer
    id="contact"
    class="footer"
  >
    <div class="footer__inner container">
      <div class="footer__grid">
        <div class="footer__brand">
          <BrandLogo size="lg" />
          <p class="footer__blurb">
            座落於山海之間的精品民宿。六間風格各異的房型,一步之遙的海岸步道,以及一杯為你手沖的早晨咖啡。
          </p>
        </div>

        <nav
          class="footer__links"
          aria-label="頁尾連結"
        >
          <h2 class="footer__heading">
            Useful Links
          </h2>
          <ul class="footer__list">
            <li
              v-for="link in links"
              :key="link.to"
            >
              <NuxtLink
                :to="link.to"
                class="footer__link"
              >
                {{ link.label }}
              </NuxtLink>
            </li>
          </ul>
        </nav>

        <div class="footer__subscribe">
          <h2 class="footer__heading">
            Subscribe
          </h2>
          <p class="footer__note">
            訂閱電子報,搶先收到季節優惠與新房型消息。
          </p>
          <form
            class="subscribe"
            @submit.prevent
          >
            <label
              for="footer-email"
              class="visually-hidden"
            >電子郵件</label>
            <input
              id="footer-email"
              v-model="email"
              type="email"
              class="subscribe__input"
              placeholder="Your Email Here"
              autocomplete="email"
            >
            <button
              type="submit"
              class="subscribe__button"
              aria-label="送出訂閱"
            >
              <img
                src="/icons/arrow-button.svg"
                alt=""
                width="40"
                height="40"
              >
            </button>
          </form>
        </div>
      </div>

      <div class="footer__social">
        <span class="footer__rule" />
        <SocialLinks />
        <span class="footer__rule" />
      </div>

      <div class="footer__bottom">
        <p>© {{ year }} Mini Stay. All Rights Reserved.</p>
        <ul class="footer__legal">
          <li><a href="#">Privacy Policy</a></li>
          <li><a href="#">Terms of Use</a></li>
        </ul>
      </div>
    </div>
  </footer>
</template>

<style lang="scss" scoped>
.footer {
  background: var(--color-grey-500);
  color: var(--color-white);
  font-size: var(--fs-small);
}

.footer__inner {
  display: flex;
  flex-direction: column;
  gap: var(--space-9);
  padding-block: var(--space-10);
}

.footer__grid {
  display: grid;
  gap: var(--space-8);

  @include up(md) {
    grid-template-columns: 1fr 1fr;
  }

  @include up(lg) {
    grid-template-columns: minmax(0, 1.4fr) minmax(112px, 0.5fr) minmax(264px, 0.9fr);
    gap: var(--space-9);
  }
}

.footer__brand {
  display: flex;
  flex-direction: column;
  gap: var(--space-6);

  @include up(md) {
    grid-column: 1 / -1;
  }

  @include up(lg) {
    grid-column: auto;
  }
}

.footer__blurb,
.footer__note {
  max-width: 34em;
  line-height: var(--lh-body);
}

.footer__heading {
  font-family: var(--font-sans);
  font-weight: 400;
  font-size: var(--fs-lead);
  line-height: 1;
  text-transform: uppercase;
  color: var(--color-gold);
  margin-block-end: var(--space-5);
}

.footer__list {
  display: flex;
  flex-direction: column;
  gap: var(--space-3);
  font-size: var(--fs-body);
  text-transform: uppercase;
}

.footer__link {
  transition: color var(--dur) var(--ease);

  &:hover {
    color: var(--color-gold);
  }
}

.footer__note {
  margin-block-end: var(--space-5);
}

.subscribe {
  display: flex;
  align-items: stretch;
  height: 40px;
  background: rgb(87 86 86 / 40%);
}

.subscribe__input {
  flex: 1 1 auto;
  min-width: 0;
  padding-inline: var(--space-4);
  background: transparent;
  border: 0;
  color: var(--color-white);
  font-weight: 300;
  font-size: var(--fs-small);

  &::placeholder {
    color: var(--color-gold);
    opacity: 1;
  }
}

.subscribe__button {
  flex: 0 0 40px;
  display: grid;
  place-items: center;
}

.footer__social {
  display: flex;
  align-items: center;
  gap: var(--space-7);

  @include up(lg) {
    gap: 83px;
  }
}

.footer__rule {
  flex: 1 1 0;
  height: 1px;
  background: var(--color-grey-100);
}

.footer__bottom {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
  align-items: center;
  text-align: center;

  @include up(md) {
    flex-direction: row;
    justify-content: space-between;
    text-align: start;
  }
}

.footer__legal {
  display: flex;
  gap: var(--space-9);

  a {
    transition: color var(--dur) var(--ease);

    &:hover {
      color: var(--color-gold);
    }
  }
}
</style>
