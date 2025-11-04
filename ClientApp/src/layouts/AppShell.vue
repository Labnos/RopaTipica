<!-- src/layouts/AppShell.vue -->
<template>
  <div class="dashboard" :class="{ 'is-collapsed': isCollapsed }">
    <!-- Sidebar -->
    <aside
      class="sidebar"
      :class="{ collapsed: isCollapsed }"
      role="navigation"
      aria-label="Barra lateral"
    >
      <div class="sidebar-header">
        <div class="brand">
          <div class="brand-mark" aria-hidden="true">CE</div>
          <h2 class="brand-text">
            <span>COMERCIALES</span><br />
            <span>EMILIAS</span>
          </h2>
        </div>

        <button
          class="toggle-btn"
          @click="toggleSidebar"
          :aria-expanded="!isCollapsed"
          aria-label="Alternar tamaño de la barra lateral"
          title="Colapsar/Expandir (Ctrl+B)"
        >
          <svg viewBox="0 0 24 24" class="chev">
            <path d="M15.41 16.59 10.83 12l4.58-4.59L14 6l-6 6 6 6z" fill="currentColor"/>
          </svg>
        </button>
      </div>

      <nav class="sidebar-nav">
        <router-link
          to="/"
          class="nav-item"
          exact-active-class="active"
          :title="isCollapsed ? 'Dashboard' : null"
        >
          <span class="icon">📊</span>
          <span class="label">Dashboard</span>
        </router-link>

        <router-link
          v-if="authStore.isAdmin"
          to="/usuarios"
          class="nav-item"
          active-class="active"
          :title="isCollapsed ? 'Usuarios' : null"
        >
          <span class="icon">👥</span>
          <span class="label">Usuarios</span>
        </router-link>

        <router-link
          to="/clientes"
          class="nav-item"
          active-class="active"
          :title="isCollapsed ? 'Clientes' : null"
        >
          <span class="icon">👤</span>
          <span class="label">Clientes</span>
        </router-link>

        <router-link
          to="/proveedores"
          class="nav-item"
          active-class="active"
          :title="isCollapsed ? 'Proveedores' : null"
        >
          <span class="icon">🏭</span>
          <span class="label">Proveedores</span>
        </router-link>

        <router-link
          to="/productos"
          class="nav-item"
          active-class="active"
          :title="isCollapsed ? 'Inventario' : null"
        >
          <span class="icon">🎨</span>
          <span class="label">Inventario</span>
        </router-link>

        <router-link
          to="/ventas"
          class="nav-item"
          active-class="active"
          :title="isCollapsed ? 'Ventas' : null"
        >
          <span class="icon">💰</span>
          <span class="label">Ventas</span>
        </router-link>

        <router-link
          to="/promociones"
          class="nav-item"
          active-class="active"
          :title="isCollapsed ? 'Promociones' : null"
        >
          <span class="icon">🎁</span>
          <span class="label">Promociones</span>
        </router-link>

        <router-link
          to="/reportes"
          class="nav-item"
          active-class="active"
          :title="isCollapsed ? 'Reportes' : null"
        >
          <span class="icon">📈</span>
          <span class="label">Reportes</span>
        </router-link>
      </nav>

      <div class="sidebar-footer">
        <div class="user-info" :title="isCollapsed ? `${authStore.userName} • ${authStore.user?.rol || ''}` : null">
          <div class="user-avatar">{{ userInitial }}</div>
          <div class="user-meta" v-show="!isCollapsed">
            <p class="user-name">{{ authStore.userName }}</p>
            <p class="user-role">{{ authStore.user?.rol }}</p>
          </div>
        </div>
        <button @click="handleLogout" class="btn-logout" :title="isCollapsed ? 'Cerrar Sesión' : null">
          <span class="label">Cerrar Sesión</span>
          <span class="icon-out" aria-hidden="true">↪</span>
        </button>
      </div>
    </aside>

    <!-- Contenido -->
    <main class="main-content">
      <RouterView />
    </main>
  </div>
</template>

<script setup>
import { computed, ref, onMounted, onBeforeUnmount } from 'vue'
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

/* === Colapsable con persistencia === */
const LS_KEY = 'sidebarCollapsed'
const isCollapsed = ref(false)

const toggleSidebar = () => {
  isCollapsed.value = !isCollapsed.value
  try { localStorage.setItem(LS_KEY, String(isCollapsed.value)) } catch {}
}

const handleResize = () => {
  if (window.innerWidth <= 1024) {
    if (!isCollapsed.value) {
      isCollapsed.value = true
      try { localStorage.setItem(LS_KEY, 'true') } catch {}
    }
  }
}

// Atajo teclado Ctrl+B
const onKey = e => {
  const key = e.key?.toLowerCase?.()
  if ((e.ctrlKey || e.metaKey) && key === 'b') {
    e.preventDefault()
    toggleSidebar()
  }
}

onMounted(() => {
  try {
    const saved = localStorage.getItem(LS_KEY)
    if (saved === 'true' || saved === 'false') {
      isCollapsed.value = saved === 'true'
    } else if (window.innerWidth <= 1024) {
      isCollapsed.value = true
    }
  } catch {}
  window.addEventListener('resize', handleResize)
  window.addEventListener('keydown', onKey)
})

onBeforeUnmount(() => {
  window.removeEventListener('resize', handleResize)
  window.removeEventListener('keydown', onKey)
})
</script>

<style scoped>
/* Variables */
:root {
  --sidebar-w: 280px;
  --sidebar-w-collapsed: 76px;
}

.dashboard {
  display: flex;
  min-height: 100vh;
  font-family: var(--font-body);
}

/* ===== SIDEBAR ===== */
.sidebar {
  width: var(--sidebar-w);
  background: var(--color-primary);
  color: white;
  display: flex;
  flex-direction: column;
  box-shadow: var(--shadow-lg);
  flex-shrink: 0;
  transition: width .25s ease;
  position: sticky;
  top: 0;
  height: 100vh;
}

.sidebar.collapsed { width: var(--sidebar-w-collapsed); }

.sidebar-header {
  padding: 16px 12px;
  display: flex; align-items: center; justify-content: space-between;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.brand { display: flex; align-items: center; gap: 12px; overflow: hidden; }
.brand-mark {
  width: 40px; height: 40px; background: var(--color-accent);
  display: grid; place-items: center; font-weight: 800; font-size: 12px;
  border-radius: var(--border-radius-md);
}
.brand-text {
  margin: 0; font-size: 16px; line-height: 1.1;
  font-family: var(--font-title); font-weight: var(--font-weight-bold);
  white-space: nowrap;
}
.sidebar.collapsed .brand-text { display: none; }

/* Botón toggle */
.toggle-btn {
  border: 1px solid rgba(255,255,255,.25);
  background: rgba(255,255,255,.12);
  color: #fff;
  border-radius: 8px;
  width: 36px; height: 36px;
  display: grid; place-items: center;
  cursor: pointer;
  transition: transform .2s ease, background .2s ease;
}
.toggle-btn:hover { background: rgba(255,255,255,.2); }
.toggle-btn .chev { width: 20px; height: 20px; transform: rotate(0deg); transition: transform .25s ease; }
.sidebar.collapsed .toggle-btn .chev { transform: rotate(180deg); }

.sidebar-nav { flex: 1; padding: 12px 6px; }
.nav-item {
  display: flex; align-items: center; gap: 12px;
  padding: 10px 12px; color: white; text-decoration: none;
  transition: background .2s ease, padding .2s ease;
  font-size: 15px; border-radius: 10px; margin: 2px 6px;
}
.nav-item:hover { background: rgba(255,255,255,.12); }
.nav-item.active { background: rgba(255,255,255,.18); outline: 2px solid var(--color-accent); }
.nav-item .icon { font-size: 18px; width: 22px; text-align: center; }
.nav-item .label { white-space: nowrap; }

.sidebar.collapsed .nav-item { justify-content: center; }
.sidebar.collapsed .nav-item .label { display: none; }

/* Footer */
.sidebar-footer { padding: 12px; border-top: 1px solid rgba(255,255,255,.1); }
.user-info { display: flex; align-items: center; gap: 12px; margin-bottom: 12px; }
.user-avatar {
  width: 40px; height: 40px; border-radius: 50%;
  background: var(--color-accent); color: white; display: flex; align-items: center; justify-content: center;
  font-weight: 800; font-size: 16px;
}
.user-meta { min-width: 0; }
.user-name { margin: 0; font-weight: 600; font-size: 14px; }
.user-role { margin: 2px 0 0 0; font-size: 12px; opacity: .85; }

.btn-logout {
  width: 100%; padding: 10px 12px;
  background: rgba(255,255,255,.16);
  border: 1px solid rgba(255,255,255,.25);
  color: white; border-radius: var(--border-radius-md);
  cursor: pointer; font-weight: 600;
  transition: background .2s ease, transform .02s ease;
  display: flex; align-items: center; justify-content: center; gap: 8px;
}
.btn-logout:hover { background: rgba(255,255,255,.22); }
.icon-out { display: none; }
.sidebar.collapsed .btn-logout .label { display: none; }
.sidebar.collapsed .btn-logout .icon-out { display: inline-block; }

/* ===== MAIN ===== */
.main-content {
  flex: 1;
  padding: 30px 40px;
  overflow-y: auto;
  background: var(--color-background);
  transition: padding .25s ease;
}

/* Responsivo */
@media (max-width: 1024px) {
  .dashboard { flex-direction: column; }
  .sidebar { width: 100%; height: auto; position: relative; }
  .sidebar.collapsed { width: 100%; }
}
</style>
