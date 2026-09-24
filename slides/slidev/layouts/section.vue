<!-- Section divider. Navy ground, gold kicker. -->
<script setup lang="ts">
defineProps<{ kicker?: string }>()
</script>

<template>
  <div class="slidev-layout layout-section">
    <div class="wrap">
      <div v-if="kicker" class="kicker">{{ kicker }}</div>
      <slot />
    </div>
  </div>
</template>

<style scoped>
.layout-section {
  background: var(--navy);
  height: 100%;
  display: flex;
  align-items: center;
  padding: 2.2rem 4rem;
}
.kicker {
  font-size: 0.78rem;
  font-weight: 800;
  letter-spacing: 0.24em;
  text-transform: uppercase;
  color: var(--gold);
  margin-bottom: 0.9rem;
}
.wrap :deep(h1) {
  color: var(--cream);
  font-size: 3rem;
  margin: 0;
}
</style>
