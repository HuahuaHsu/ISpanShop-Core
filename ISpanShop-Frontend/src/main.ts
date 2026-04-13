import './assets/main.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'

//啟用 Element Plus 並設定中文
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import zhTw from 'element-plus/es/locale/lang/zh-tw'  // 繁體中文
import Vue3ApexCharts from "vue3-apexcharts";

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.use(Vue3ApexCharts)

//註冊元件
app.use(ElementPlus, {
    locale: zhTw,
})

app.mount('#app')
