<template>
  <div class="space-y-2">
    <div class="flex gap-2 items-center mb-2">
      <select v-model="filter" class="border p-1">
        <option value="all">Alles</option>
        <option value="todo">Te doen</option>
        <option value="done">Voltooid</option>
      </select>
    </div>
    <div v-if="exercises.length === 0" class="text-sm text-gray-500">Geen oefeningen gevonden.</div>
    <div v-for="ex in filtered" :key="ex.id">
      <ExerciseItem :exercise="ex" @toggled="onToggled" />
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue';
import ExerciseItem from './ExerciseItem.vue';
const props = defineProps({ exercises: { type: Array, default: () => [] } });
const emit = defineEmits(['update']);
const filter = ref('all');

const filtered = computed(() => {
  if (filter.value === 'all') return props.exercises;
  return props.exercises.filter(e => e.status === filter.value);
});

function onToggled() {
  emit('update');
}
</script>
