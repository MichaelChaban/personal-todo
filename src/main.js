import { createApp } from 'vue'
import { Quasar, Notify, Dialog } from 'quasar'
import router from './router'
import App from './App.vue'

// Import Quasar styles
import 'quasar/dist/quasar.css'
import '@quasar/extras/material-icons/material-icons.css'

const app = createApp(App)

app.use(Quasar, {
  plugins: {
    Notify,
    Dialog
  },
  config: {
    notify: {
      position: 'top-right',
      timeout: 2500
    }
  }
})

app.use(router)
app.mount('#app')
