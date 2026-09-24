<!--
  Full-bleed code. No headline: the code is the slide.
  `codeSize` is computed by the generator from the widest line so nothing is clipped.
-->
<script setup lang="ts">
defineProps<{ codeSize?: string }>()
</script>

<template>
  <div class="slidev-layout layout-code" :style="{ '--code-size': (codeSize ?? '15') + 'px' }">
    <div class="code-stack">
      <slot />
    </div>
  </div>
</template>

<style scoped>
.layout-code {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 100%;
  padding: 1.8rem 2rem;
}
.code-stack {
  width: 100%;
  max-width: 53rem;
  display: flex;
  flex-direction: column;
  gap: 0.9rem;
}
.code-stack :deep(.slidev-code),
.code-stack :deep(.slidev-code code) {
  font-size: var(--code-size);
}
</style>
