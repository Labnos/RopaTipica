<template>
  <div class="dashboard">
    <!-- Sidebar -->
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

    <!-- Main Content -->
    <main class="main-content">
      <!-- Header con selector de período -->
      <div class="dashboard-header">
        <h1>Dashboard</h1>
        <div class="period-selector">
          <label>Periodo:</label>
          <select v-model="selectedPeriod" class="period-dropdown">
            <option value="today">Hoy</option>
            <option value="week">Esta Semana</option>
            <option value="month">Este Mes</option>
            <option value="year">Este Año</option>
          </select>
        </div>
      </div>

      <!-- Tarjetas de estadísticas principales -->
      <div class="stats-grid">
        <div class="stat-card">
          <h3>INGRESOS HOY</h3>
          <p class="stat-value primary">Q 2,175.50</p>
          <p class="stat-subtitle">Basado en 18 ventas</p>
          <button class="btn btn-primary btn-sm">Ver reporte →</button>
        </div>

        <div class="stat-card">
          <h3>VENTAS DEL DÍA</h3>
          <p class="stat-value">18</p>
          <p class="stat-subtitle">12 tienda, 6, online</p>
          <button class="btn btn-primary btn-sm">Ver reporte →</button>
        </div>

        <div class="stat-card">
          <h3>PEDIDOS PENDIENTES</h3>
          <p class="stat-value">5</p>
          <p class="stat-subtitle">Por pagar o entregar</p>
          <button class="btn btn-primary btn-sm">Registrar venta •</button>
        </div>

        <div class="stat-card">
          <h3>BAJO INVENTARIO</h3>
          <p class="stat-value">9</p>
          <p class="stat-subtitle">Productos a agotar</p>
          <button class="btn btn-primary btn-sm">Ver pedidos →</button>
        </div>
      </div>

      <h2 class="section-title">Este mes</h2>

      <!-- Segunda fila: Gráficos -->
      <div class="charts-grid">
        <!-- Gráfico de Ventas de la Semana -->
        <div class="chart-card">
          <div class="chart-header">
            <h3>📈 Ventas de la Semana</h3>
            <select class="chart-filter">
              <option>Sucursal: Todas</option>
              <option>Sucursal Central</option>
              <option>Sucursal Antigua</option>
            </select>
          </div>
          <div class="chart-container">
            <canvas ref="weekSalesChart"></canvas>
          </div>
        </div>

        <!-- Gráfico de Canales de Venta -->
        <div class="chart-card">
          <div class="chart-header">
            <h3>🕐 Canales de Venta (Hoy)</h3>
          </div>
          <div class="chart-container pie-chart">
            <canvas ref="channelsChart"></canvas>
            <div class="chart-legend">
              <div class="legend-item">
                <span class="legend-color" style="background: #1B4B7F"></span>
                <span>Tienda Física ... 67%</span>
              </div>
              <div class="legend-item">
                <span class="legend-color" style="background: #FF8C42"></span>
                <span>Facebook ... 22%</span>
              </div>
              <div class="legend-item">
                <span class="legend-color" style="background: #10B981"></span>
                <span>WhatsApp: ... 11%</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Tercera fila: Información adicional -->
      <div class="info-grid">
        <!-- Alertas y Notificaciones -->
        <div class="info-card">
          <h3>🔔 Alertas y Notificaciones</h3>
          <ul class="notification-list">
            <li>[Bajo Stock] Huipil Coban (2)</li>
            <li>[Pedido] Envío para Ana G.</li>
            <li>[Promoción] Cortes rebajados</li>
          </ul>
        </div>

        <!-- Productos Más Vendidos -->
        <div class="info-card">
          <h3>🔗 Productos Más Vendidos (Mes)</h3>
          <ol class="products-list">
            <li>Corte Totonicapán (38)</li>
            <li>Blusa Quiché (90)</li>
            <li>Huipil Nebaj (23)</li>
          </ol>
        </div>

        <!-- Actividad Reciente -->
        <div class="info-card">
          <h3>⚡ Actividad Reciente</h3>
          <ul class="activity-list">
            <li>› Venta #10S registrar Tienda (1)</li>
            <li>› (Venta #99)</li>
            <li>› Proveedor Textiles Maya (Detall)</li>
          </ul>
        </div>
      </div>
    </main>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useAuthStore } from '../stores/auth.js'
import { useRouter } from 'vue-router'
import { useToast } from 'vue-toastification'

const authStore = useAuthStore()
const router = useRouter()
const toast = useToast()

const selectedPeriod = ref('today')
const weekSalesChart = ref(null)
const channelsChart = ref(null)

const userInitial = computed(() => {
  return authStore.userName.charAt(0).toUpperCase()
})

const handleLogout = async () => {
  await authStore.logout()
  toast.info('Sesión cerrada')
  router.push('/login')
}

// Inicializar gráficos (placeholder - necesitarás Chart.js)
onMounted(() => {
  // Aquí irían los gráficos con Chart.js
  // Por ahora solo un placeholder
})
</script>

<style scoped>
.dashboard {
  display: flex;
  min-height: 100vh;
  font-family: var(--font-body);
}

/* ===== SIDEBAR ===== */
.sidebar {
  width: 280px;
  background: var(--color-primary);
  color: white;
  display: flex;
  flex-direction: column;
  box-shadow: var(--shadow-lg);
  flex-shrink: 0;
}

.sidebar-header {
  padding: 30px 20px;
  text-align: center;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.logo-small {
  width: 50px;
  height: 50px;
  background: var(--color-accent);
  margin: 0 auto 15px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
  font-size: 12px;
  border-radius: var(--border-radius-md);
}

.sidebar-header h2 {
  margin: 0;
  font-size: 18px;
  font-family: var(--font-title);
  line-height: 1.3;
  font-weight: var(--font-weight-bold);
}

.sidebar-nav {
  flex: 1;
  padding: 20px 0;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 20px;
  color: white;
  text-decoration: none;
  transition: all 0.3s;
  font-size: 15px;
}

.nav-item:hover {
  background: rgba(255, 255, 255, 0.1);
}

.nav-item.active {
  background: rgba(255, 255, 255, 0.2);
  border-left: 4px solid var(--color-accent);
}

.nav-item .icon {
  font-size: 18px;
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
  background: var(--color-accent);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
  font-size: 16px;
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
  border-radius: var(--border-radius-md);
  cursor: pointer;
  font-weight: 500;
  transition: all 0.3s;
  font-size: 14px;
}

.btn-logout:hover {
  background: rgba(255, 255, 255, 0.3);
}

/* ===== MAIN CONTENT ===== */
.main-content {
  flex: 1;
  padding: 30px 40px;
  overflow-y: auto;
  background: var(--color-background);
}

.dashboard-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 30px;
}

.dashboard-header h1 {
  margin: 0;
  font-family: var(--font-title);
  color: var(--color-primary);
  font-size: 32px;
}

.period-selector {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 14px;
}

.period-selector label {
  color: var(--color-text-primary);
  font-weight: var(--font-weight-medium);
}

.period-dropdown {
  padding: 8px 16px;
  border: 1px solid var(--color-border);
  border-radius: var(--border-radius-md);
  background: white;
  font-family: var(--font-body);
  font-size: 14px;
  cursor: pointer;
}

/* ===== TARJETAS DE ESTADÍSTICAS ===== */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
  gap: 20px;
  margin-bottom: 40px;
}

.stat-card {
  background: white;
  border: 2px solid var(--color-border);
  border-radius: var(--border-radius-lg);
  padding: 24px;
  text-align: center;
}

.stat-card h3 {
  font-family: var(--font-body);
  font-size: 14px;
  font-weight: var(--font-weight-semibold);
  color: var(--color-text-primary);
  margin: 0 0 12px 0;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.stat-value {
  font-family: var(--font-title);
  font-size: 42px;
  font-weight: var(--font-weight-bold);
  color: var(--color-primary);
  margin: 0 0 8px 0;
  line-height: 1;
}

.stat-value.primary {
  color: var(--color-primary);
}

.stat-subtitle {
  font-size: 13px;
  color: var(--color-text-secondary);
  margin: 0 0 16px 0;
}

.btn-sm {
  padding: 8px 16px;
  font-size: 13px;
}

/* ===== SECCIÓN ===== */
.section-title {
  font-family: var(--font-title);
  font-size: 20px;
  color: var(--color-text-primary);
  margin: 0 0 20px 0;
}

/* ===== GRÁFICOS ===== */
.charts-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
  gap: 20px;
  margin-bottom: 30px;
}

.chart-card {
  background: white;
  border: 2px solid var(--color-border);
  border-radius: var(--border-radius-lg);
  padding: 24px;
}

.chart-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.chart-header h3 {
  font-family: var(--font-body);
  font-size: 16px;
  font-weight: var(--font-weight-semibold);
  color: var(--color-primary);
  margin: 0;
}

.chart-filter {
  padding: 6px 12px;
  border: 1px solid var(--color-border);
  border-radius: var(--border-radius-md);
  font-size: 13px;
  background: white;
}

.chart-container {
  min-height: 250px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-text-muted);
}

.pie-chart {
  display: flex;
  gap: 30px;
}

.chart-legend {
  display: flex;
  flex-direction: column;
  gap: 12px;
  justify-content: center;
}

.legend-item {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 14px;
}

.legend-color {
  width: 16px;
  height: 16px;
  border-radius: 3px;
  flex-shrink: 0;
}

/* ===== INFO GRID ===== */
.info-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 20px;
}

.info-card {
  background: white;
  border: 2px solid var(--color-border);
  border-radius: var(--border-radius-lg);
  padding: 24px;
}

.info-card h3 {
  font-family: var(--font-body);
  font-size: 16px;
  font-weight: var(--font-weight-semibold);
  color: var(--color-primary);
  margin: 0 0 16px 0;
}

.notification-list,
.products-list,
.activity-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.notification-list li,
.activity-list li {
  padding: 10px 0;
  border-bottom: 1px solid var(--color-background);
  font-size: 14px;
  color: var(--color-text-secondary);
}

.notification-list li:last-child,
.activity-list li:last-child {
  border-bottom: none;
}

.products-list {
  counter-reset: item;
  padding-left: 0;
}

.products-list li {
  padding: 10px 0;
  padding-left: 30px;
  border-bottom: 1px solid var(--color-background);
  font-size: 14px;
  color: var(--color-text-secondary);
  position: relative;
  counter-increment: item;
}

.products-list li:before {
  content: counter(item);
  position: absolute;
  left: 0;
  font-weight: var(--font-weight-bold);
  color: var(--color-primary);
}

.products-list li:last-child {
  border-bottom: none;
}

/* ===== RESPONSIVE ===== */
@media (max-width: 1024px) {
  .charts-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 768px) {
  .dashboard {
    flex-direction: column;
  }

  .sidebar {
    width: 100%;
  }

  .stats-grid {
    grid-template-columns: 1fr;
  }

  .info-grid {
    grid-template-columns: 1fr;
  }
}
</style>