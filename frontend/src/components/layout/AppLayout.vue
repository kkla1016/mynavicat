<template>
  <div class="app-layout">
    <Sidebar />
    <div class="main-container">
      <Header />
      <main class="content-area">
        <router-view />
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
import Sidebar from './Sidebar.vue'
import Header from './Header.vue'
</script>

<style scoped lang="scss">
.app-layout {
  display: flex;
  width: 100vw;
  height: 100vh;
  background-color: #121824;
  overflow: hidden;

  .main-container {
    flex: 1;
    display: flex;
    flex-direction: column;
    height: 100%;
    overflow: hidden;

    .content-area {
      flex: 1;
      overflow-y: auto;
      background-color: #121824;
    }
  }
}
</style>
