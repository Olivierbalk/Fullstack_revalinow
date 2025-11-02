import axios from 'axios';
import { API_BASE } from '../config/api';

const api = axios.create({
  baseURL: API_BASE,
  headers: { 'Content-Type': 'application/json' },
  timeout: 15000,
});

// Attach JWT token from localStorage
api.interceptors.request.use((config) => {
  try {
    const token = localStorage.getItem('jwt_token');
    if (token) config.headers.Authorization = `Bearer ${token}`;
  } catch (e) {
    // ignore in non-browser environments
  }
  return config;
}, (error) => Promise.reject(error));

// Optional: global response handler for 401
api.interceptors.response.use((r) => r, (error) => {
  if (error?.response?.status === 401) {
    // let application handle redirect via store or router guard
    // no direct router import here to avoid circular deps
    localStorage.removeItem('jwt_token');
  }
  return Promise.reject(error);
});

export default api;
