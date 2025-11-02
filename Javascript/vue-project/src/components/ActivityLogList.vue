<template>
  <div class="space-y-2 mt-4">
    <div v-if="items.length === 0" class="text-sm text-gray-500">Geen items.</div>
    <div v-for="it in items" :key="it.id" class="p-3 bg-white rounded border flex justify-between items-start">
      <div>
        <div class="font-semibold">{{ it.title }}</div>
        <div class="text-sm text-gray-600">{{ it.description }}</div>
        <div class="text-xs text-gray-500">{{ formatDate(it.createdAt) }}</div>
      </div>
      <div class="flex flex-col gap-2">
        <button @click="$emit('updated', it.id)" class="text-sm text-blue-600">Bewerken</button>
        <button @click="remove(it.id)" class="text-sm text-red-600">Verwijderen</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { useActivityStore } from '../stores/activity';
const props = defineProps({ items: { type: Array, default: () => [] } });
const store = useActivityStore();

function formatDate(d) { return new Date(d).toLocaleString(); }

async function remove(id) {
  if (!confirm('Verwijderen?')) return;
  await store.remove(id);
  // notify parent
  // emit handled in template
}
</script>
