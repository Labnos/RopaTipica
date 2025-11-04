<!-- src/layouts/AppShell.vue -->
<template>
  <div class="dashboard">
    <!-- Sidebar (reutilizable en todas las vistas) -->
    <aside class="sidebar">
      <div class="sidebar-header">
        <h2>COMERCIALES<br>EMILIAS</h2>
      </div>

      <nav class="sidebar-nav">
        <router-link to="/" class="nav-item" exact-active-class="active">
          <span class="icon">📊</span>
          <span>Dashboard</span>
        </router-link>

        <router-link v-if="authStore.isAdmin" to="/usuarios" class="nav-item" active-class="active">
          <span class="icon">👥</span>
          <span>Usuarios</span>
        </router-link>

        <router-link to="/clientes" class="nav-item" active-class="active">
          <span class="icon">👤</span>
          <span>Clientes</span>
        </router-link>

        <router-link to="/proveedores" class="nav-item" active-class="active">
          <span class="icon">🏭</span>
          <span>Proveedores</span>
        </router-link>

        <router-link to="/productos" class="nav-item" active-class="active">
          <span class="icon">🎨</span>
          <span>Inventario</span>
        </router-link>

        <router-link to="/ventas" class="nav-item" active-class="active">
          <span class="icon">💰</span>
          <span>Ventas</span>
        </router-link>

        <router-link to="/promociones" class="nav-item" active-class="active">
          <span class="icon">🎁</span>
          <span>Promociones</span>
        </router-link>

        <router-link to="/reportes" class="nav-item" active-class="active">
          <span class="icon">📈</span>
          <span>Reportes</span>
        </router-link>
      </nav>

      <div class="sidebar-footer">
        <div class="user-info">
          <div class="user-avatar">{{ userInitial }}</div>
          <div>
            <p class="user-name">{{ authStore.userName }}</p>
            <p class="user-role">{{ authStore.user?.rol }}</p>
          </div>
        </div>
        <button @click="handleLogout" class="btn-logout">
          Cerrar Sesión
        </button>
      </div>
    </aside>

    <!-- Contenedor principal donde se pintan las páginas -->
    <main class="main-content">
      <RouterView />
    </main>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'vue-toastification'
import { useAuthStore } from '@/stores/auth.js'

const authStore = useAuthStore()
const router = useRouter()
const toast = useToast()

const userInitial = computed(() => (authStore.userName || '?').charAt(0).toUpperCase())

const handleLogout = async () => {
  await authStore.logout()
  toast.info('Sesión cerrada')
  router.push('/login')
}
</script>

<style scoped>
/* ——— Copiado de tu dashboard.vue (se mantiene igual) ——— */
.dashboard { display: flex; min-height: 100vh; font-family: var(--font-body); }

/* SIDEBAR */
.sidebar { width: 280px; background: var(--color-primary); color: white; display: flex; flex-direction: column; box-shadow: var(--shadow-lg); flex-shrink: 0; }
.sidebar-header { padding: 30px 20px; text-align: center; border-bottom: 1px solid rgba(255, 255, 255, 0.1); }
.sidebar-header h2 { margin: 0; font-size: 18px; font-family: var(--font-title); line-height: 1.3; font-weight: var(--font-weight-bold); }
.sidebar-nav { flex: 1; padding: 20px 0; }
.nav-item { display: flex; align-items: center; gap: 12px; padding: 12px 20px; color: white; text-decoration: none; transition: all 0.3s; font-size: 15px; }
.nav-item:hover { background: rgba(255, 255, 255, 0.1); }
.nav-item.active { background: rgba(255, 255, 255, 0.2); border-left: 4px solid var(--color-accent); }
.nav-item .icon { font-size: 18px; }
.sidebar-footer { padding: 20px; border-top: 1px solid rgba(255, 255, 255, 0.1); }
.user-info { display: flex; align-items: center; gap: 12px; margin-bottom: 16px; }
.user-avatar { width: 40px; height: 40px; border-radius: 50%; background: var(--color-accent); color: white; display: flex; align-items: center; justify-content: center; font-weight: bold; font-size: 16px; }
.user-name { margin: 0; font-weight: 600; font-size: 14px; }
.user-role { margin: 2px 0 0 0; font-size: 12px; opacity: 0.8; }
.btn-logout { width: 100%; padding: 10px; background: rgba(255, 255, 255, 0.2); border: 1px solid rgba(255, 255, 255, 0.3); color: white; border-radius: var(--border-radius-md); cursor: pointer; font-weight: 500; transition: all 0.3s; font-size: 14px; }
.btn-logout:hover { background: rgba(255, 255, 255, 0.3); }

/* MAIN */
.main-content { flex: 1; padding: 30px 40px; overflow-y: auto; background: var(--color-background); }

/* Responsivo (igual que tu archivo) */
@media (max-width: 1024px) { .charts-grid { grid-template-columns: 1fr; } }
@media (max-width: 768px) {
  .dashboard { flex-direction: column; }
  .sidebar { width: 100%; }
}
</style>
