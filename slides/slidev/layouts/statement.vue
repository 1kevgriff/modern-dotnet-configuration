<!-- Headline-only. One claim, nothing else on screen. -->
<template>
  <div class="slidev-layout layout-statement">
    <div class="wrap"><slot /></div>
  </div>
</template>

<style scoped>
.layout-statement {
  height: 100%;
  display: flex;
  align-items: center;
  padding: 2.2rem 4.5rem;
}
.wrap { max-width: 46rem; }
.wrap :deep(p),
.wrap :deep(blockquote p) {
  font-size: 2.1rem;
  font-weight: 800;
  line-height: 1.22;
  letter-spacing: -0.02em;
  color: var(--navy);
  margin: 0 0 0.6rem;
}
.wrap :deep(blockquote) { border-left: 4px solid var(--gold); padding-left: 1.6rem; }
</style>
