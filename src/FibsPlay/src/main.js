// The Vue build version to load with the `import` command
// (runtime-only or standalone) has been set in webpack.base.conf with an alias.
import Vue from 'vue'
import Router from 'vue-router'
import App from '@/components/App'
import Login from '@/components/Login'
import Who from '@/components/Who'
import FibsClient from './FibsClient';

Vue.use(Router)
Vue.config.productionTip = false

/* eslint-disable no-new */
new Vue({
  el: '#app',
  router: new Router({
    routes: [
      { name: "login", path: '/', component: Login },
      { name: "who", path: '/who', component: Who },
    ],
    mode: "history",
  }),
  template: '<App/>',
  components: { App },
  data: { client: new FibsClient() },
});
