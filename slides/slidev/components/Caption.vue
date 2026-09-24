<script setup lang="ts">
defineProps<{ gold?: boolean }>()
</script>

<template>
  <div class="caption" :class="{ gold }"><slot /></div>
</template>

<style scoped>
.caption {
  margin-top: 0.7rem;
  font-size: 0.86rem;
  color: var(--muted);
  line-height: 1.4;
}
.caption.gold {
  color: var(--navy);
  font-weight: 800;
  font-size: 0.98rem;
  border-left: 4px solid var(--gold);
  padding-left: 0.8rem;
}
.caption :deep(code) {
  background: rgba(22, 48, 63, 0.07);
  padding: 0.08em 0.34em;
  border-radius: 4px;
}
</style>
