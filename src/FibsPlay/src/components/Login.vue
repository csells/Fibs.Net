<template>
  <div>
    <form v-on:submit="login">
      <div><span>user: <input v-model="user" :disabled=inputDisabled /></span></div>
      <div><span>password: <input v-model="pass" type="password" :disabled=inputDisabled /></span></div>
      <div>
        <input type="submit" v-model="submitName" :disabled=submitDisabled />
        <router-link to="who" v-if="whoVisible">Who</router-link>
      </div>
      <div>status: {{client.session.status}}</div>
      <div>error: {{client.session.error}}</div>
      <div>last login: {{client.session.lastLogin}}</div>
      <div>last host: {{client.session.lastHost}}</div>
    </form>
    <h1 v-if="client.session.motd">MOTD</h1>
    <p v-for="(line, i) in client.session.motd" :key="i">{{line}}</p>
  </div>
</template>

<script>
export default {
  data: function() {
    return {
      user: "dotnetcli", // HACK: devmode
      pass: "dotnetcli", // HACK: devmode
      client: this.$root.$data.client
    };
  },

  computed: {
    submitName: function() {
      return this.client.session.status === "closed" ? "Login" : "Logout";
    },

    submitDisabled: function() {
      return (
        this.client.session.status === "closed" &&
        (this.user === "" || this.pass === "")
      );
    },

    inputDisabled: function() {
      return this.client.session.status !== "closed";
    },

    whoVisible: function() {
      return this.client.session.status === "authenticated";
    }
  },

  methods: {
    login: function(e) {
      e.preventDefault();
      if (this.client.session.status === "closed") {
        this.client.login(this.user, this.pass);
      } else {
        this.client.logout();
      }
    }
  }
};
</script>
