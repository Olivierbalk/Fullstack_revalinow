<template>
  <div :class="['p-3 border rounded flex justify-between items-start', exercise.status === 'done' ? 'opacity-60 line-through' : '']">
    <div>
      <div class="font-semibold">{{ exercise.name }}</div>
      <div class="text-sm text-gray-600">{{ exercise.description }}</div>
      <div class="text-xs text-gray-500 mt-1">Sets: {{ exercise.sets }} • Reps: {{ exercise.reps }}</div>
      <div v-if="exercise.video" class="mt-2">
        <a :href="exercise.video" target="_blank" class="text-blue-600 text-sm">Bekijk video</a>
      </div>
    </div>
    <div class="flex flex-col items-end gap-2">
      <button @click="toggle" class="px-3 py-1 bg-green-500 text-white rounded">
        {{ exercise.status === 'done' ? 'Undo' : 'Afgerond' }}
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { useExercisesStore } from '../stores/exercises';
const props = defineProps({ exercise: Object });
const emit = defineEmits(['toggled']);
const loading = ref(false);
const store = useExercisesStore();

async function toggle() {
  if (loading.value) return;
  loading.value = true;
  try {
    const newStatus = props.exercise.status === 'done' ? 'todo' : 'done';
    await store.toggleStatus(props.exercise.id, newStatus);
    props.exercise.status = newStatus; // optimistic update
    emit('toggled');
  } catch (e) {
    alert('Status update mislukt');
  } finally {
    loading.value = false;
  }
}
</script>
