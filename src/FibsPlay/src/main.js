// The Vue build version to load with the `import` command
// (runtime-only or standalone) has been set in webpack.base.conf with an alias.
import Vue from 'vue'
import Router from 'vue-router'
import App from '@/components/App'
import Login from '@/components/Login'

Vue.use(Router)
Vue.config.productionTip = false

/* eslint-disable no-new */
new Vue({
  el: '#app',
  router: new Router({
    routes: [{
      path: '/',
      component: Login
    },
    ],
    mode: "history",
  }),
  template: '<App/>',
  components: { App },
});
