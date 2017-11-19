<template>
  <form v-on:submit="login">
      <div><span>user: <input v-model="user" :disabled=inputDisabled /></span></div>
      <div><span>password: <input v-model="pass" type="password" :disabled=inputDisabled /></span></div>
      <div><input type="submit" v-model="submitName" :disabled=submitDisabled /></div>
      <div>status: {{session.status}}</div>
      <div>error: {{session.error}}</div>
      <div>last login: {{session.lastLogin}}</div>
      <div>last host: {{session.lastHost}}</div>
    </form>
  </template>

<script>
import Session from "../session";

export default {
  data() {
    return {
      user: "dotnetcli", // HACK: devmode
      pass: "dotnetcli", // HACK: devmode
      session: new Session()
    };
  },

  computed: {
    submitName: function() {
      return this.session.status === "closed" ? "Login" : "Logout";
    },

    submitDisabled: function() {
      return (
        this.session.status === "closed" &&
        (this.user === "" || this.pass === "")
      );
    },

    inputDisabled: function() {
      return this.session.status === "opened";
    }
  },

  methods: {
    login: function(e) {
      e.preventDefault();
      if (this.session.status === "closed") {
        this.session.login(this.user, this.pass);
      } else {
        this.session.logout();
      }
    }
  }
};
</script>
