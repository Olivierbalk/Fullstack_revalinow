import { defineStore } from 'pinia';
import api from '../utils/apiClient';

export const usePainStore = defineStore('pain', {
  state: () => ({
    logs: [],
    loading: false,
  }),
  actions: {
    async fetch() {
      this.loading = true;
      try {
        const res = await api.get('/pain-logs');
        this.logs = res.data;
      } finally {
        this.loading = false;
      }
    },
    async create(payload) {
      const res = await api.post('/pain-logs', payload);
      this.logs.unshift(res.data);
      return res.data;
    }
  }
});
