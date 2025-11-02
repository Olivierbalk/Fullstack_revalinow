<template>
  <div class="space-y-2">
    <div v-if="exercises.length === 0" class="text-sm text-gray-500">Geen oefeningen gepland.</div>
    <div v-for="e in exercises" :key="e.id" class="text-sm">
      <div class="flex justify-between">
        <div>{{ e.name }}</div>
        <div class="text-gray-500">{{ e.sets }}x{{ e.reps }}</div>
      </div>
    </div>
  </div>
</template>

<script setup>
const props = defineProps({ exercises: { type: Array, default: () => [] } });
</script>
