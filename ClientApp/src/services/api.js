import axios from 'axios'

// Determinar la URL base según el entorno
const API_BASE_URL = import.meta.env.DEV 
  ? '/api'  // En desarrollo, usa el proxy de Vite
  : '/api'  // En producción, relativo a la raíz

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json'
  },
  // Aumentar timeout para operaciones lentas
  timeout: 30000
})

// Interceptor de solicitudes
api.interceptors.request.use(
  config => {
    const token = localStorage.getItem('token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    
    // Log para debugging
    console.log(`API Request: ${config.method.toUpperCase()} ${config.baseURL}${config.url}`)
    
    return config
  },
  error => {
    console.error('Request interceptor error:', error)
    return Promise.reject(error)
  }
)

// Interceptor de respuestas
api.interceptors.response.use(
  response => {
    console.log(`API Response: ${response.status} ${response.config.url}`)
    return response
  },
  error => {
    console.error('Response interceptor error:', error)
    
    if (error.response) {
      // El servidor respondió con un estado de error
      console.error('Error response:', error.response.status, error.response.data)
      
      if (error.response.status === 401) {
        // Token expirado o inválido
        localStorage.removeItem('token')
        localStorage.removeItem('user')
        localStorage.removeItem('tokenVersion')
        localStorage.removeItem('expiresAt')
        window.location.href = '/login'
      } else if (error.response.status === 403) {
        console.error('Acceso denegado (403)')
      } else if (error.response.status === 404) {
        console.error('Recurso no encontrado (404)')
      } else if (error.response.status >= 500) {
        console.error('Error del servidor (5xx)')
      }
    } else if (error.request) {
      // La solicitud se realizó pero no hubo respuesta
      console.error('No response received:', error.request)
    } else {
      // Error en la configuración de la solicitud
      console.error('Error setting up request:', error.message)
    }
    
    return Promise.reject(error)
  }
)

export default api