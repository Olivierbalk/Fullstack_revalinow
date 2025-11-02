<template>
  <div class="space-y-2">
    <div v-if="items.length === 0" class="text-sm text-gray-500">Geen afspraken.</div>
    <div v-for="a in items" :key="a.id" class="p-3 bg-white rounded border">
      <AppointmentItem :appointment="a" />
    </div>
  </div>
</template>

<script setup>
import AppointmentItem from './AppointmentItem.vue';
const props = defineProps({ items: { type: Array, default: () => [] } });
</script>
