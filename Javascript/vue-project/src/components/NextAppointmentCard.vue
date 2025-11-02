<template>
  <div v-if="appointment">
    <div class="font-semibold">{{ appointment.title }}</div>
    <div class="text-sm text-gray-600">{{ formatted }}</div>
    <div class="text-xs mt-2">{{ appointment.location }}</div>
  </div>
  <div v-else class="text-sm text-gray-500">Geen afspraken gepland.</div>
</template>

<script setup>
import { computed } from 'vue';
const props = defineProps({ appointment: Object });
const formatted = computed(() => {
  if (!props.appointment) return '';
  return new Date(props.appointment.startsAt).toLocaleString();
});
</script>
