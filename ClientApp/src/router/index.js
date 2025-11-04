import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const routes = [
  {
    path: '/login',
    name: 'Login',
    component: () => import('../views/Login.vue'),
    meta: { requiresAuth: false }
  },
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: () => import('../views/Dashboard.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/usuarios',
    name: 'Usuarios',
    component: () => import('../views/Usuarios.vue'),
    meta: { requiresAuth: true, roles: ['Administrador'] }
  },
  {
    path: '/clientes',
    name: 'Clientes',
    component: () => import('../views/Clientes.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/proveedores',
    name: 'Proveedores',
    component: () => import('../views/Proveedores.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/productos',
    name: 'Productos',
    component: () => import('../views/Productos.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/ventas',
    name: 'Ventas',
    component: () => import('../views/Ventas.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/ventas/nueva',
    name: 'NuevaVenta',
    component: () => import('../views/NuevaVenta.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/promociones',
    name: 'Promociones',
    component: () => import('../views/Promociones.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/reportes',
    name: 'Reportes',
    component: () => import('../views/Reportes.vue'),
    meta: { requiresAuth: true }
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach(async (to, from, next) => {
  const authStore = useAuthStore()
  
  if (to.meta.requiresAuth) {
    if (!authStore.isAuthenticated) {
      next('/login')
      return
    }

    if (to.meta.roles && to.meta.roles.length > 0) {
      const userRole = authStore.user?.rol
      if (!to.meta.roles.includes(userRole)) {
        next('/')
        return
      }
    }
  }

  if (to.path === '/login' && authStore.isAuthenticated) {
    next('/')
    return
  }

  next()
})

export default router