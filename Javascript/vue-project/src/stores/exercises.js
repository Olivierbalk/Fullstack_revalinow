import { defineStore } from 'pinia';
import api from '../utils/apiClient';

export const useExercisesStore = defineStore('exercises', {
  state: () => ({
    items: [],
    loading: false,
  }),
  actions: {
    async fetch() {
      this.loading = true;
      try {
        const res = await api.get('/exercises');
        this.items = res.data;
      } finally {
        this.loading = false;
      }
    },
    async toggleStatus(id, newStatus) {
      await api.patch(`/exercises/${id}/status`, { status: newStatus });
      const idx = this.items.findIndex(i => i.id === id);
      if (idx !== -1) this.items[idx].status = newStatus;
    }
  }
});
