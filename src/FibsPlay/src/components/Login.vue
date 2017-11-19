<template>
  <form v-on:submit="login">
      <div><span>user: <input v-model="user" /></span></div>
      <div><span>password: <input v-model="pw" type="password" /></span></div>
      <div><input type="submit" value="login" /></div>
      <div>status: {{status}}</div>
      <div>error: {{error}}</div>
      <div>last login: {{lastLogin}}</div>
      <div>last host: {{lastHost}}</div>
    </form>
  </template>

<script>
export default {
  data() {
    return {
      user: "",
      pw: "",
      status: "ok",
      error: "none",
      lastLogin: "unknown",
      lastHost: "unknown"
    };
  },
  methods: {
    login: function(e) {
      e.preventDefault();
      alert(`user= '${this.user}', pw=${this.pw}`);
    }
  }
};
</script>

<style scoped>

</style>
