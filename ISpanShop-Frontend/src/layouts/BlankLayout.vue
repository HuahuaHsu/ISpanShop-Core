<!-- src/layouts/BlankLayout.vue -->
<template>
  <div class="layout">
    <header class="header">
      <div class="header-inner">
        <div class="logo" @click="$router.push('/')">HowBuy</div>
        <div class="title">{{ pageTitle }}</div>
      </div>
    </header>
    <main class="main">
      <router-view />
    </main>
    <footer class="footer">© 2026 HowBuy</footer>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
const route = useRoute()
const pageTitle = computed(() => {
  if (route.path.includes('login')) return '登入'
  if (route.path.includes('register')) return '註冊'
  return ''
})
</script>

<style scoped>
.layout { min-height: 100vh; display: flex; flex-direction: column; background: #f1f5f9; }
.header { background: white; padding: 30px 0; border-bottom: 3px solid #EE4D2D; }
.header-inner {
  max-width: 1400px;
  margin: 0 auto;
  padding: 0 30px;
  display: flex;
  align-items: baseline;
  gap: 20px;
}
.logo { color: #EE4D2D; font-size: 32px; font-weight: bold; cursor: pointer; }
.title { color: #475569; font-size: 20px; }
.main { flex: 1; display: flex; align-items: center; justify-content: center; padding: 40px 20px; }
.footer { text-align: center; padding: 20px; color: #94a3b8; font-size: 13px; }
</style>
