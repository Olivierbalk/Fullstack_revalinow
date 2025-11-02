<template>
  <div class="min-h-screen flex bg-gray-50">
    <aside class="w-64 bg-white border-r p-4">
      <div class="font-bold mb-4">ReveliNow</div>
      <nav class="flex flex-col space-y-2">
        <router-link to="/dashboard" class="text-sm">Dashboard</router-link>
        <router-link to="/exercises" class="text-sm">Oefeningen</router-link>
        <router-link to="/pain" class="text-sm">Pijnregistratie</router-link>
        <router-link to="/activity-log" class="text-sm">Logboek</router-link>
        <router-link to="/appointments" class="text-sm">Afspraken</router-link>
      </nav>
      <div class="mt-6">
        <button @click="logout" class="text-sm text-red-600">Logout</button>
      </div>
    </aside>
    <main class="flex-1 p-6">
      <router-view />
    </main>
  </div>
</template>

<script setup>
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/auth';
const auth = useAuthStore();
const router = useRouter();
function logout() {
  auth.logout();
  router.push('/auth/login');
}
</script>
