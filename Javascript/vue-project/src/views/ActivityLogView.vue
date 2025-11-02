<template>
  <div>
    <h2 class="text-xl mb-4">Logboek</h2>
    <ActivityLogForm @saved="load" />
    <ActivityLogList :items="items" @deleted="load" @updated="load" />
  </div>
</template>

<script setup>
import { onMounted } from 'vue';
import { useActivityStore } from '../stores/activity';
import ActivityLogForm from '../components/ActivityLogForm.vue';
import ActivityLogList from '../components/ActivityLogList.vue';

const store = useActivityStore();
const items = store.items;

async function load() {
  await store.fetch();
}

onMounted(load);
</script>
