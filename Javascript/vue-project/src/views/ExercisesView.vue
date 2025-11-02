<template>
  <div>
    <h2 class="text-xl mb-4">Oefeningen</h2>
    <ExerciseList :exercises="exercises" @update="load" />
  </div>
</template>

<script setup>
import { onMounted } from 'vue';
import { useExercisesStore } from '../stores/exercises';
import ExerciseList from '../components/ExerciseList.vue';

const store = useExercisesStore();
const exercises = store.items;

async function load() {
  await store.fetch();
}

onMounted(load);
</script>
