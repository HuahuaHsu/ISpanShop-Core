import './assets/main.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'

//啟用 Element Plus 並設定中文
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import zhTw from 'element-plus/es/locale/lang/zh-tw'  // 繁體中文
import './styles/theme.css' // 覆寫主色調為橘紅色 #EE4D2D

const app = createApp(App)

app.use(createPinia())
app.use(router)

//註冊元件
app.use(ElementPlus, {
    locale: zhTw,
})

app.mount('#app')
