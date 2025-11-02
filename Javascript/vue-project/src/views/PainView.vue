<template>
  <div>
    <h2 class="text-xl mb-4">Pijnregistratie</h2>
    <PainLogForm @saved="loadLogs" />
    <div class="mt-6 bg-white p-4 rounded shadow">
      <h3 class="font-semibold mb-2">Grafiek</h3>
      <PainChart :logs="logs" />
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { usePainStore } from '../stores/pain';
import PainLogForm from '../components/PainLogForm.vue';
import PainChart from '../components/PainChart.vue';

const store = usePainStore();
const logs = store.logs;

async function loadLogs() {
  await store.fetch();
}

onMounted(loadLogs);
</script>
