<template>
  <div id="app">
    <header>
      <span id="banner">Play FIBS!</span>
    </header>
    <main>
      <router-view></router-view>
    </main>
  </div>
</template>

<style scoped>
body {
  margin: 0;
}

#app header {
  color: white;
  background-color: black;
  padding: 5px;
}
</style>
