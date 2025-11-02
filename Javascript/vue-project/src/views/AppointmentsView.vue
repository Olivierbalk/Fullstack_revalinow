<template>
  <div>
    <h2 class="text-xl mb-4">Afspraken</h2>
    <div class="mb-4">
      <label class="mr-2"><input type="checkbox" v-model="showPast" /> Toon verleden</label>
    </div>
    <AppointmentsList :items="items" />
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue';
import { useAppointmentsStore } from '../stores/appointments';
import AppointmentsList from '../components/AppointmentsList.vue';

const store = useAppointmentsStore();
const items = store.items;
const showPast = ref(false);

async function load() {
  await store.fetch({ past: showPast.value });
}

onMounted(load);
watch(showPast, load);
</script>
