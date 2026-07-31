<template>
  <div class="app-wrapper">
    <CustomTitlebar />
    <div class="app-layout">
      <Sidebar />
      <div class="main-container">
        <Header />
        <div class="content-area">
          <router-view />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import Sidebar from './Sidebar.vue'
import Header from './Header.vue'
import CustomTitlebar from './CustomTitlebar.vue'
</script>

<style scoped lang="scss">
.app-wrapper {
  display: flex;
  flex-direction: column;
  height: 100vh;
  width: 100vw;
  overflow: hidden;
}

.app-layout {
  display: flex;
  flex: 1;
  overflow: hidden;

  .main-container {
    display: flex;
    flex-direction: column;
    flex: 1;
    overflow: hidden;

    .content-area {
      flex: 1;
      overflow-y: auto;
      background-color: #121824;
    }
  }
}
</style>
