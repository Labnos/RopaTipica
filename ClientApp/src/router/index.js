// src/router/index.js
import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/auth'

// Lazy-load del layout (sidebar reutilizable)
const AppShell = () => import('../layouts/AppShell.vue')

const routes = [
  // Rutas públicas (sin sidebar)
  {
    path: '/login',
    name: 'Login',
    component: () => import('../views/Login.vue'),
    meta: { requiresAuth: false }
  },

  // Rutas privadas con SIDEBAR (todas las child comparten AppShell)
  {
    path: '/',
    component: AppShell,
    meta: { requiresAuth: true }, // se aplica a todos los children
    children: [
      { path: '', name: 'Dashboard', component: () => import('../views/Dashboard.vue') },

      // Puedes seguir controlando roles por vista:
      { path: 'usuarios', name: 'Usuarios', component: () => import('../views/Usuarios.vue'), meta: { roles: ['Administrador'] } },

      { path: 'clientes', name: 'Clientes', component: () => import('../views/Clientes.vue') },
      { path: 'proveedores', name: 'Proveedores', component: () => import('../views/Proveedores.vue') },
      { path: 'productos', name: 'Productos', component: () => import('../views/Productos.vue') },
      { path: 'ventas', name: 'Ventas', component: () => import('../views/Ventas.vue') },
      { path: 'ventas/nueva', name: 'NuevaVenta', component: () => import('../views/NuevaVenta.vue') },
      { path: 'promociones', name: 'Promociones', component: () => import('../views/Promociones.vue') },
      { path: 'reportes', name: 'Reportes', component: () => import('../views/Reportes.vue') },
    ]
  },

  // (Opcional) 404
  // { path: '/:pathMatch(.*)*', name: 'NotFound', component: () => import('../views/NotFound.vue') }
]

const router = createRouter({
  history: createWebHistory(),
  routes,
  // (Opcional) mantener scroll arriba al navegar
  scrollBehavior() { return { top: 0 } }
})

router.beforeEach(async (to, from, next) => {
  const authStore = useAuthStore()

  // Requiere auth si cualquier record matcheado lo pide
  const needsAuth = to.matched.some(r => r.meta?.requiresAuth)
  if (needsAuth && !authStore.isAuthenticated) {
    return next('/login')
  }

  // Control de roles: si alguna ruta hija declara roles, validamos
  const routeWithRoles = to.matched.find(r => r.meta?.roles && r.meta.roles.length > 0)
  if (routeWithRoles) {
    const allowed = routeWithRoles.meta.roles
    const userRole = authStore.user?.rol
    if (!allowed.includes(userRole)) {
      return next('/') // o a una página 403 si la tienes
    }
  }

  // Si ya está logueado, no permitir ir a /login
  if (to.path === '/login' && authStore.isAuthenticated) {
    return next('/')
  }

  next()
})

export default router
