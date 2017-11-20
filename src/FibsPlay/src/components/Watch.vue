<template>
  <div>
    watch {{$route.params.name}}
  </div>
</template>

<script>
export default {
  data() {
    return {
      client: this.$root.$data.client
    };
  },
};
</script>
