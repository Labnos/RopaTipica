<template>
  <div class="dashboard">
    <!-- Sidebar -->
    <aside class="sidebar">
      <div class="sidebar-header">
        <h2>🇬🇹 Inventario</h2>
        <p>Ropa Típica</p>
      </div>

      <nav class="sidebar-nav">
        <router-link to="/" class="nav-item" exact-active-class="active">
          <span class="icon">📊</span>
          <span>Dashboard</span>
        </router-link>

        <router-link 
          v-if="authStore.isAdmin" 
          to="/usuarios" 
          class="nav-item"
          active-class="active"
        >
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

    <!-- Main Content -->
    <main class="main-content">
      <div class="content-header">
        <h1>Dashboard</h1>
      </div>

      <div class="stats-grid">
        <div class="stat-card">
          <div class="stat-icon" style="background: #667eea">💰</div>
          <div class="stat-info">
            <h3>Ventas del Mes</h3>
            <p class="stat-value">Q 0.00</p>
          </div>
        </div>

        <div class="stat-card">
          <div class="stat-icon" style="background: #f093fb">🎨</div>
          <div class="stat-info">
            <h3>Productos</h3>
            <p class="stat-value">0</p>
          </div>
        </div>

        <div class="stat-card">
          <div class="stat-icon" style="background: #4facfe">👥</div>
          <div class="stat-info">
            <h3>Clientes</h3>
            <p class="stat-value">0</p>
          </div>
        </div>

        <div class="stat-card">
          <div class="stat-icon" style="background: #43e97b">📦</div>
          <div class="stat-info">
            <h3>Bajo Stock</h3>
            <p class="stat-value">0</p>
          </div>
        </div>
      </div>

      <div class="welcome-section">
        <h2>¡Bienvenido, {{ authStore.userName }}! 👋</h2>
        <p>Sistema de Inventario de Ropa Típica Guatemalteca</p>
        <div class="quick-actions">
          <router-link to="/ventas/nueva" class="action-btn primary">
            ➕ Nueva Venta
          </router-link>
          <router-link to="/productos" class="action-btn">
            📦 Ver Inventario
          </router-link>
          <router-link to="/reportes" class="action-btn">
            📊 Ver Reportes
          </router-link>
        </div>
      </div>
    </main>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useAuthStore } from '../stores/auth'
import { useRouter } from 'vue-router'
import { useToast } from 'vue-toastification'

const authStore = useAuthStore()
const router = useRouter()
const toast = useToast()

const userInitial = computed(() => {
  return authStore.userName.charAt(0).toUpperCase()
})

const handleLogout = async () => {
  await authStore.logout()
  toast.info('Sesión cerrada')
  router.push('/login')
}
</script>

<style scoped>
.dashboard {
  display: flex;
  min-height: 100vh;
  background: #f5f7fa;
}

.sidebar {
  width: 280px;
  background: linear-gradient(180deg, #667eea 0%, #764ba2 100%);
  color: white;
  display: flex;
  flex-direction: column;
  box-shadow: 4px 0 12px rgba(0, 0, 0, 0.1);
}

.sidebar-header {
  padding: 30px 20px;
  text-align: center;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.sidebar-header h2 {
  margin: 0;
  font-size: 24px;
}

.sidebar-header p {
  margin: 4px 0 0 0;
  opacity: 0.9;
  font-size: 14px;
}

.sidebar-nav {
  flex: 1;
  padding: 20px 0;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 14px 20px;
  color: white;
  text-decoration: none;
  transition: all 0.3s;
}

.nav-item:hover {
  background: rgba(255, 255, 255, 0.1);
}

.nav-item.active {
  background: rgba(255, 255, 255, 0.2);
  border-left: 4px solid white;
}

.nav-item .icon {
  font-size: 20px;
}

.sidebar-footer {
  padding: 20px;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
}

.user-info {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;
}

.user-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: white;
  color: #667eea;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
  font-size: 18px;
}

.user-name {
  margin: 0;
  font-weight: 600;
  font-size: 14px;
}

.user-role {
  margin: 2px 0 0 0;
  font-size: 12px;
  opacity: 0.8;
}

.btn-logout {
  width: 100%;
  padding: 10px;
  background: rgba(255, 255, 255, 0.2);
  border: 1px solid rgba(255, 255, 255, 0.3);
  color: white;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 500;
  transition: all 0.3s;
}

.btn-logout:hover {
  background: rgba(255, 255, 255, 0.3);
}

.main-content {
  flex: 1;
  padding: 40px;
  overflow-y: auto;
}

.content-header h1 {
  margin: 0 0 30px 0;
  color: #333;
  font-size: 32px;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 24px;
  margin-bottom: 40px;
}

.stat-card {
  background: white;
  border-radius: 12px;
  padding: 24px;
  display: flex;
  align-items: center;
  gap: 16px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  transition: transform 0.3s;
}

.stat-card:hover {
  transform: translateY(-4px);
}

.stat-icon {
  width: 60px;
  height: 60px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 28px;
}

.stat-info h3 {
  margin: 0;
  font-size: 14px;
  color: #666;
  font-weight: 500;
}

.stat-value {
  margin: 4px 0 0 0;
  font-size: 28px;
  font-weight: bold;
  color: #333;
}

.welcome-section {
  background: white;
  border-radius: 12px;
  padding: 40px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.welcome-section h2 {
  margin: 0 0 8px 0;
  color: #333;
  font-size: 28px;
}

.welcome-section p {
  margin: 0 0 24px 0;
  color: #666;
  font-size: 16px;
}

.quick-actions {
  display: flex;
  gap: 16px;
  flex-wrap: wrap;
}

.action-btn {
  padding: 12px 24px;
  border-radius: 8px;
  text-decoration: none;
  font-weight: 500;
  transition: all 0.3s;
  background: #f5f7fa;
  color: #333;
}

.action-btn.primary {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
}

.action-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}
</style>