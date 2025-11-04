import api from './api'

export default {
  async login(credentials) {
    const response = await api.post('/Auth/Login', credentials)
    if (response.data.success) {
      localStorage.setItem('token', response.data.token)
      localStorage.setItem('user', JSON.stringify(response.data.user))
      localStorage.setItem('tokenVersion', response.data.tokenVersion)
      localStorage.setItem('expiresAt', response.data.expiresAt)
    }
    return response.data
  },

  async logout() {
    try {
      await api.post('/Auth/Logout')
    } catch (error) {
      console.error('Error en logout:', error)
    } finally {
      localStorage.removeItem('token')
      localStorage.removeItem('user')
      localStorage.removeItem('tokenVersion')
      localStorage.removeItem('expiresAt')
    }
  },

  async getCurrentUser() {
    const response = await api.get('/Auth/Me')
    return response.data
  },

  async validateToken() {
    try {
      const response = await api.get('/Auth/ValidateToken')
      return response.data.success
    } catch (error) {
      return false
    }
  },

  isAuthenticated() {
    const token = localStorage.getItem('token')
    const expiresAt = localStorage.getItem('expiresAt')
    
    if (!token || !expiresAt) return false
    
    const now = new Date()
    const expiration = new Date(expiresAt)
    
    return now < expiration
  },

  getUser() {
    const userStr = localStorage.getItem('user')
    return userStr ? JSON.parse(userStr) : null
  }
}