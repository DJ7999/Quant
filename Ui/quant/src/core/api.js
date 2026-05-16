// src/core/api.js
const BASE_URL = import.meta.env.VITE_API_BASE_URL;

export const api = {
  // Helper for POST requests with files
  upload: async (endpoint, formData) => {
    const response = await fetch(`${BASE_URL}${endpoint}`, {
      method: 'POST',
      body: formData,
      // Note: No 'Content-Type' header here, browser handles boundary
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Server Error' }));
      throw new Error(error.message || `HTTP error! status: ${response.status}`);
    }
    return response.json();
  },

  // Generic POST helper for JSON data
  post: async (endpoint, data) => {
    const response = await fetch(`${BASE_URL}${endpoint}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(data),
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Server Error' }));
      throw new Error(error.message || `HTTP error! status: ${response.status}`);
    }
    return response.json();
  },

  // Generic GET helper
  get: async (endpoint) => {
    const response = await fetch(`${BASE_URL}${endpoint}`);
    if (!response.ok) throw new Error('Network response was not ok');
    return response.json();
  }
};