import axios from 'axios'

const API = axios.create({
  baseURL: 'https://localhost:7025/api/v1',
})

// Automatically attach token to every request
API.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Handle errors
API.interceptors.response.use(
  (response) => response,
  (error) => {
    // Only logout if server explicitly returns 401
    // NOT if backend is down (network error = no response)
    if (error.response && error.response.status === 401) {
      localStorage.removeItem('token')
      localStorage.removeItem('role')
      localStorage.removeItem('userId')
      localStorage.removeItem('name')
      window.location.href = '/'  // ← redirect to landing, not login
    }
    return Promise.reject(error)
  }
)

export default API