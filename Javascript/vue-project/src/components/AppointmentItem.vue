<template>
  <div>
    <div class="font-semibold">{{ appointment.title }}</div>
    <div class="text-sm text-gray-600">{{ new Date(appointment.startsAt).toLocaleString() }}</div>
    <div class="text-xs mt-1 text-gray-500">{{ appointment.location }}</div>
  </div>
</template>

<script setup>
const props = defineProps({ appointment: Object });
</script>
