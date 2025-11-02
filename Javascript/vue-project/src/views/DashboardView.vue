<template>
  <div>
    <header class="flex justify-between items-center mb-6">
      <h1 class="text-2xl">Welkom, {{ userName }}</h1>
      <div class="flex gap-2">
        <router-link to="/exercises" class="btn">Oefeningen</router-link>
        <router-link to="/activity-log" class="btn">Logboek</router-link>
        <router-link to="/pain" class="btn">Pijnregistratie</router-link>
        <router-link to="/appointments" class="btn">Afspraken</router-link>
      </div>
    </header>

    <section class="grid grid-cols-3 gap-4">
      <div class="col-span-2 p-4 bg-white rounded shadow">
        <h3 class="font-semibold mb-2">Recente pijnindicaties</h3>
        <PainChart :logs="painLogs" />
      </div>

      <aside class="p-4 bg-white rounded shadow space-y-4">
        <div>
          <h3 class="font-semibold">Eerstvolgende afspraak</h3>
          <NextAppointmentCard :appointment="nextAppointment" />
        </div>
        <div>
          <h3 class="font-semibold">Vandaag / deze week</h3>
          <ExerciseListMini :exercises="todayExercises" />
        </div>
      </aside>
    </section>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useAuthStore } from '../stores/auth';
import api from '../utils/apiClient';
import PainChart from '../components/PainChart.vue';
import NextAppointmentCard from '../components/NextAppointmentCard.vue';
import ExerciseListMini from '../components/ExerciseListMini.vue';

const auth = useAuthStore();
const userName = auth.userName;
const painLogs = ref([]);
const todayExercises = ref([]);
const nextAppointment = ref(null);

async function load() {
  try {
    const res = await api.get('/dashboard');
    painLogs.value = res.data.painLogs || [];
    todayExercises.value = res.data.todayExercises || [];
    nextAppointment.value = res.data.nextAppointment || null;
  } catch (e) {
    console.error(e);
  }
}

onMounted(load);
</script>
