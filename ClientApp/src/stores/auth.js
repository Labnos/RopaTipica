import { defineStore } from 'pinia'
import authService from '../services/authService'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: authService.getUser(),
    token: localStorage.getItem('token'),
    isAuthenticated: authService.isAuthenticated()
  }),

  getters: {
    isAdmin: (state) => state.user?.rol === 'Administrador',
    isVendedor: (state) => state.user?.rol === 'Vendedor',
    isEncargado: (state) => state.user?.rol === 'Encargado',
    userName: (state) => state.user?.nombre || '',
    userEmail: (state) => state.user?.email || ''
  },

  actions: {
    async login(credentials) {
      try {
        const response = await authService.login(credentials)
        
        if (response.success) {
          this.user = response.user
          this.token = response.token
          this.isAuthenticated = true
          return { success: true, message: response.message }
        }
        
        return { success: false, message: response.message }
      } catch (error) {
        console.error('Error en login:', error)
        return { 
          success: false, 
          message: error.response?.data?.message || 'Error al iniciar sesión' 
        }
      }
    },

    async logout() {
      try {
        await authService.logout()
      } catch (error) {
        console.error('Error en logout:', error)
      } finally {
        this.user = null
        this.token = null
        this.isAuthenticated = false
      }
    },

    async checkAuth() {
      if (!authService.isAuthenticated()) {
        this.logout()
        return false
      }

      try {
        const isValid = await authService.validateToken()
        if (!isValid) {
          this.logout()
          return false
        }
        return true
      } catch (error) {
        this.logout()
        return false
      }
    }
  }
})