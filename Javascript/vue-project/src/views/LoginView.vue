<template>
  <div>
    <h2 class="text-2xl mb-4">Inloggen</h2>
    <form @submit.prevent="onSubmit" class="space-y-4">
      <div>
        <label class="block text-sm">Email</label>
        <input v-model="email" class="w-full border p-2" required />
      </div>
      <div>
        <label class="block text-sm">Wachtwoord</label>
        <input v-model="password" type="password" class="w-full border p-2" required />
      </div>
      <div>
        <button class="bg-blue-600 text-white px-4 py-2" :disabled="loading">
          {{ loading ? 'Bezig...' : 'Login' }}
        </button>
      </div>
    </form>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { useAuthStore } from '../stores/auth';

const email = ref('');
const password = ref('');
const auth = useAuthStore();
const router = useRouter();
const route = useRoute();
const loading = ref(false);

async function onSubmit() {
  loading.value = true;
  try {
    await auth.login({ email: email.value, password: password.value });
    const redirect = route.query.redirect || '/dashboard';
    router.push(redirect);
  } catch (err) {
    // TODO: toast
    alert(err?.response?.data?.message || 'Login failed');
  } finally {
    loading.value = false;
  }
}
</script>
