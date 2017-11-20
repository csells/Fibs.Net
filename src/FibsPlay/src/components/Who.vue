<template>
  <div>
    <div v-for="person in people">
      <div>{{person.name}}</div>
    </div>
  </div>
</template>

<script>
export default {
  data() {
    return {
      filter: null,
      client: this.$root.$data.client
    };
  },

  computed: {
    people: function() {
      return [...this.client.who]
        .sort((a, b) => a.name.toLowerCase().localeCompare(b.name.toLowerCase()))
        .filter(p => p.name !== this.client.session.user)
      ;
    }
  },

  methods: {
  }
};
</script>
