<script setup lang="ts">
// `size` is a px font-size computed by the generator from the widest code line, so panel code
// fits its column instead of spilling out of the panel. See fitPanels() in outline-to-slidev.mjs.
defineProps<{ cols?: number | string; size?: string }>()
</script>

<template>
  <div
    class="panel-row"
    :style="{ '--cols': String(cols ?? 2), '--code-size': (size ?? '13') + 'px' }"
  >
    <slot />
  </div>
</template>

<style scoped>
.panel-row {
  display: grid;
  grid-template-columns: repeat(var(--cols, 2), minmax(0, 1fr));
  gap: 0.85rem;
  align-items: stretch;
  min-width: 0;
}
</style>
