import { defineStore } from 'pinia';
import api from '../utils/apiClient';

export const useAppointmentsStore = defineStore('appointments', {
  state: () => ({
    items: [],
    loading: false,
  }),
  actions: {
    async fetch({past=false} = {}) {
      this.loading = true;
      try {
        const res = await api.get('/appointments', { params: { past } });
        this.items = res.data;
      } finally {
        this.loading = false;
      }
    }
  }
});
