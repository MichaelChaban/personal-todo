<template>
  <q-page class="flex flex-center">
    <div class="text-center">
      <q-icon name="construction" size="8rem" color="grey-5" />
      <h5 class="q-mt-md q-mb-sm">Coming Soon</h5>
      <p class="text-grey-7">This feature is under development</p>
      <q-btn
        outline
        color="primary"
        label="Go Back"
        icon="arrow_back"
        @click="$router.back()"
        class="q-mt-md"
      />
    </div>
  </q-page>
</template>

<style scoped>
h5 {
  font-size: 1.5rem;
  font-weight: 600;
  margin: 0;
}
</style>
