// src/core/api.js
const BASE_URL = import.meta.env.VITE_API_BASE_URL;

// Helper function to safely parse JSON from response
const parseResponse = async (response) => {
  const contentType = response.headers.get('content-type');
  
  // If no content or empty body, return null or empty object
  if (!response.body || response.status === 204 || response.status === 304) {
    return null;
  }
  
  // Only parse as JSON if content-type indicates JSON
  if (contentType && contentType.includes('application/json')) {
    try {
      const text = await response.text();
      return text ? JSON.parse(text) : null;
    } catch (e) {
      throw new Error('Invalid JSON response from server');
    }
  }
  
  // For non-JSON responses, return the text
  return response.text();
};

export const api = {
  // Helper for POST requests with files
  upload: async (endpoint, formData) => {
    const response = await fetch(`${BASE_URL}${endpoint}`, {
      method: 'POST',
      body: formData,
      // Note: No 'Content-Type' header here, browser handles boundary
    });

    if (!response.ok) {
      const error = await parseResponse(response).catch(() => ({ message: 'Server Error' }));
      throw new Error(error?.message || `HTTP error! status: ${response.status}`);
    }
    return parseResponse(response);
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
      const error = await parseResponse(response).catch(() => ({ message: 'Server Error' }));
      throw new Error(error?.message || `HTTP error! status: ${response.status}`);
    }
    return parseResponse(response);
  },

  // Generic GET helper
  get: async (endpoint) => {
    const response = await fetch(`${BASE_URL}${endpoint}`);
    if (!response.ok) throw new Error('Network response was not ok');
    return parseResponse(response);
  }
};