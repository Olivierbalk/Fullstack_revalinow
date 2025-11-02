<template>
  <form @submit.prevent="submit" class="bg-white p-4 rounded shadow">
    <div class="mb-2">
      <label class="block text-sm">Titel</label>
      <input v-model="title" class="w-full border p-2" required />
    </div>
    <div class="mb-2">
      <label class="block text-sm">Beschrijving</label>
      <textarea v-model="description" class="w-full border p-2"></textarea>
    </div>
    <div>
      <button class="bg-blue-600 text-white px-3 py-1">Toevoegen</button>
    </div>
  </form>
</template>

<script setup>
import { ref } from 'vue';
import { useActivityStore } from '../stores/activity';
const emit = defineEmits(['saved']);
const title = ref('');
const description = ref('');
const store = useActivityStore();

async function submit() {
  try {
    await store.create({ title: title.value, description: description.value });
    title.value = '';
    description.value = '';
    emit('saved');
  } catch (e) {
    alert('Opslaan mislukt');
  }
}
</script>
