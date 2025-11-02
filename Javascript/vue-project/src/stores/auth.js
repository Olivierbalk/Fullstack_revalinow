import { defineStore } from 'pinia';
import api from '../utils/apiClient';

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null,
    token: localStorage.getItem('jwt_token') || null,
    loading: false,
  }),
  getters: {
    isAuthenticated: (state) => !!state.token,
    userName: (state) => state.user?.name || state.user?.displayName || '',
  },
  actions: {
    setToken(token) {
      this.token = token;
      if (token) localStorage.setItem('jwt_token', token);
      else localStorage.removeItem('jwt_token');
    },
    setUser(user) {
      this.user = user;
    },
    async login(credentials) {
      this.loading = true;
      try {
        const res = await api.post('/auth/login', credentials);
        const { token, user } = res.data;
        this.setToken(token);
        this.setUser(user || null);
        return res.data;
      } finally {
        this.loading = false;
      }
    },
    logout() {
      this.setUser(null);
      this.setToken(null);
    },
  },
});
