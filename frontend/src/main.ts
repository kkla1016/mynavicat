import { createApp } from 'vue'
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import 'element-plus/theme-chalk/dark/css-vars.css'
import { createPinia } from 'pinia'
import router from './router'
import App from './App.vue'
import './styles/global.scss'
import './styles/element-overrides.scss'

const app = createApp(App)

// 強制啟用 Element Plus 深色主題
document.documentElement.classList.add('dark')

app.use(createPinia())
app.use(router)
app.use(ElementPlus)

app.mount('#app')
