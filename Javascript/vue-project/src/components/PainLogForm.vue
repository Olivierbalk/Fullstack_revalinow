<template>
  <form @submit.prevent="submit" class="bg-white p-4 rounded shadow space-y-3">
    <div>
      <label class="block mb-1">Pijn (0–10): <strong>{{ value }}</strong></label>
      <input type="range" min="0" max="10" v-model="value" class="w-full" />
    </div>
    <div>
      <label class="block mb-1">Opmerking (optioneel)</label>
      <textarea v-model="note" class="w-full border p-2"></textarea>
    </div>
    <div>
      <button class="bg-blue-600 text-white px-3 py-1" :disabled="loading">{{ loading ? 'Opslaan...' : 'Opslaan' }}</button>
    </div>
  </form>
</template>

<script setup>
import { ref } from 'vue';
import { usePainStore } from '../stores/pain';
const emit = defineEmits(['saved']);
const value = ref(5);
const note = ref('');
const loading = ref(false);
const store = usePainStore();

async function submit() {
  if (value.value < 0 || value.value > 10) return alert('Waarde moet tussen 0 en 10 liggen');
  loading.value = true;
  try {
    await store.create({ value: value.value, note: note.value });
    value.value = 5; note.value = '';
    emit('saved');
  } catch (e) {
    alert('Opslaan mislukt');
  } finally {
    loading.value = false;
  }
}
</script>
