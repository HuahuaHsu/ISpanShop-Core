import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'
// 👇 1. 引入剛剛下載的 SSL 套件
import basicSsl from '@vitejs/plugin-basic-ssl'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    vueDevTools(),
    // basicSsl() // 👇 2. 啟用 SSL 套件
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  },
  // 👇 這是我們剛剛講的新增區塊 👇
  server: {
    port: 5173,       // 強制綁定 5173
    strictPort: true, // 嚴格模式，被佔用就報錯
    proxy: {
      '/api': {
        // ⚠️ 記得把這裡的 7000 換成你們 C# 後端實際運行的 Port！
        target: 'https://localhost:7125',
        changeOrigin: true,
        secure: false,
      }
    }
  }
})
