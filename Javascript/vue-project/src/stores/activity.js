import { defineStore } from 'pinia';
import api from '../utils/apiClient';

export const useActivityStore = defineStore('activity', {
  state: () => ({
    items: [],
    loading: false,
  }),
  actions: {
    async fetch() {
      this.loading = true;
      try {
        const res = await api.get('/activity-logs');
        this.items = res.data;
      } finally {
        this.loading = false;
      }
    },
    async create(payload) {
      const res = await api.post('/activity-logs', payload);
      this.items.unshift(res.data);
      return res.data;
    },
    async update(id, payload) {
      const res = await api.patch(`/activity-logs/${id}`, payload);
      const idx = this.items.findIndex(i => i.id === id);
      if (idx !== -1) this.items[idx] = res.data;
      return res.data;
    },
    async remove(id) {
      await api.delete(`/activity-logs/${id}`);
      this.items = this.items.filter(i => i.id !== id);
    }
  }
});
