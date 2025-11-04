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
          <select v-model="selectedPeriod" class="period-dropdown" @change="reloadPeriod">
            <option value="today">Hoy</option>
            <option value="week" disabled>Esta Semana</option>
            <option value="month">Este Mes</option>
            <option value="year">Este Año</option>
          </select>
        </div>
      </div>

      <!-- Tarjetas de estadísticas principales -->
      <div class="stats-grid">
        <div class="stat-card">
          <h3>INGRESOS HOY</h3>
          <p class="stat-value primary">Q {{ formatMoney(resumen.ingresosHoy) }}</p>
          <p class="stat-subtitle">Ventas completadas hoy</p>
          <button class="btn btn-primary btn-sm" @click="goToReportes">Ver reporte →</button>
        </div>

        <div class="stat-card">
          <h3>VENTAS DEL DÍA</h3>
          <p class="stat-value">{{ resumen.ventasHoy }}</p>
          <p class="stat-subtitle">Órdenes cerradas</p>
          <button class="btn btn-primary btn-sm" @click="goToVentas">Ver ventas →</button>
        </div>

        <div class="stat-card">
          <h3>PEDIDOS PENDIENTES</h3>
          <p class="stat-value">{{ resumen.ventasPendientes }}</p>
          <p class="stat-subtitle">Por pagar o entregar</p>
          <button class="btn btn-primary btn-sm" @click="goToVentas">Revisar •</button>
        </div>

        <div class="stat-card">
          <h3>BAJO INVENTARIO</h3>
          <p class="stat-value">{{ bajoStockCount }}</p>
          <p class="stat-subtitle">Productos a agotar</p>
          <button class="btn btn-primary btn-sm" @click="goToProductos">Ver productos →</button>
        </div>
      </div>

      <h2 class="section-title">Analítica</h2>

      <!-- Segunda fila: Gráficos -->
      <div class="charts-grid">
        <!-- Ventas Mensuales (Línea) -->
        <div class="chart-card">
          <div class="chart-header">
            <h3>📈 Ventas Mensuales ({{ currentYear }})</h3>
            <select class="chart-filter" v-model="currentYear" @change="loadVentasMensuales">
              <option v-for="y in years" :key="y" :value="y">{{ y }}</option>
            </select>
          </div>
          <div class="chart-container">
            <canvas ref="ventasMensualesCanvas"></canvas>
          </div>
        </div>

        <!-- Top Productos (Barras) -->
        <div class="chart-card">
          <div class="chart-header">
            <h3>🏆 Top Productos Vendidos</h3>
            <select class="chart-filter" v-model.number="topN" @change="loadTopProductos">
              <option :value="3">Top 3</option>
              <option :value="5">Top 5</option>
              <option :value="10">Top 10</option>
            </select>
          </div>
          <div class="chart-container">
            <canvas ref="topProductosCanvas"></canvas>
          </div>
        </div>
      </div>

      <!-- Tercera fila: Información adicional -->
      <div class="info-grid">
        <!-- Alertas y Notificaciones -->
        <div class="info-card">
          <h3>🔔 Alertas y Notificaciones</h3>
          <ul class="notification-list">
            <li v-if="bajoStockCount > 0">[Bajo Stock] {{ bajoStockCount }} producto(s) necesitan reposición</li>
            <li>[Info] Recuerda verificar ventas pendientes</li>
            <li>[Info] Revisa promociones activas</li>
          </ul>
        </div>

        <!-- Productos Más Vendidos (Lista) -->
        <div class="info-card">
          <h3>🔗 Productos Más Vendidos</h3>
          <ol class="products-list">
            <li v-for="p in topProductos" :key="p.producto">
              {{ p.producto }} ({{ p.cantidadVendida }})
            </li>
          </ol>
        </div>

        <!-- Actividad Reciente (placeholder) -->
        <div class="info-card">
          <h3>⚡ Actividad Reciente</h3>
          <ul class="activity-list">
            <li>› Nuevas ventas registradas</li>
            <li>› Productos actualizados</li>
            <li>› Revisión de pendientes</li>
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
import Chart from 'chart.js/auto'
import dashboardService from '../services/dashboardService'

const authStore = useAuthStore()
const router = useRouter()
const toast = useToast()

const selectedPeriod = ref('today')

const resumen = ref({
  ingresosHoy: 0,
  ventasHoy: 0,
  ventasPendientes: 0,
  productosBajoStock: 0
})

const bajoStock = ref([])
const bajoStockCount = computed(() => Array.isArray(bajoStock.value?.data) ? bajoStock.value.data.length : (resumen.value.productosBajoStock || 0))

const topProductos = ref([])

const ventasMensuales = ref([])

const ventasMensualesCanvas = ref(null)
const topProductosCanvas = ref(null)

let ventasMensualesChart = null
let topProductosChart = null

const years = ref([])
const now = new Date()
const currentYear = ref(now.getFullYear())
const topN = ref(5)

const userInitial = computed(() => (authStore.userName || '?').charAt(0).toUpperCase())

const handleLogout = async () => {
  await authStore.logout()
  toast.info('Sesión cerrada')
  router.push('/login')
}

const goToVentas = () => router.push('/ventas')
const goToProductos = () => router.push('/productos')
const goToReportes = () => router.push('/reportes')

function formatMoney(n) {
  const num = Number(n || 0)
  return num.toLocaleString('es-GT', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

function buildYears() {
  const y = now.getFullYear()
  years.value = [y - 2, y - 1, y, y + 1]
}

async function loadResumen() {
  try {
    const res = await dashboardService.getResumen()
    if (res.success && res.data) {
      // ApiResponse con { success, data: { ... } }
      resumen.value = res.data
    } else if (res.data?.data) {
      // Por si tu ApiResponse viene doblemente anidada
      resumen.value = res.data.data
    }
  } catch (e) {
    console.error('Error cargando resumen', e)
  }
}

async function loadBajoStock() {
  try {
    const res = await dashboardService.getBajoStock()
    bajoStock.value = res
  } catch (e) {
    console.error('Error cargando bajo stock', e)
  }
}

async function loadTopProductos() {
  try {
    const res = await dashboardService.getTopProductos(topN.value)
    // Espera ApiResponse<List<TopProductoDto>>
    const list = res.data || res?.Data || []
    topProductos.value = list

    // gráfico
    const labels = list.map(x => x.producto || x.Producto)
    const values = list.map(x => x.cantidadVendida ?? x.CantidadVendida ?? 0)

    if (topProductosChart) topProductosChart.destroy()
    topProductosChart = new Chart(topProductosCanvas.value.getContext('2d'), {
      type: 'bar',
      data: {
        labels,
        datasets: [{
          label: 'Cantidad vendida',
          data: values
        }]
      },
      options: {
        responsive: true,
        plugins: { legend: { display: true } },
        scales: { y: { beginAtZero: true } }
      }
    })
  } catch (e) {
    console.error('Error cargando top productos', e)
  }
}

async function loadVentasMensuales() {
  try {
    const res = await dashboardService.getVentasMensuales(currentYear.value)
    const list = res.data || res?.Data || []
    // Normalizar: [{ Mes, Total }]
    const monthMap = new Map(list.map(x => [Number(x.Mes ?? x.mes), Number(x.Total ?? x.total)]))
    const labels = ['Ene','Feb','Mar','Abr','May','Jun','Jul','Ago','Sep','Oct','Nov','Dic']
    const values = Array.from({ length: 12 }, (_, i) => monthMap.get(i + 1) || 0)

    if (ventasMensualesChart) ventasMensualesChart.destroy()
    ventasMensualesChart = new Chart(ventasMensualesCanvas.value.getContext('2d'), {
      type: 'line',
      data: {
        labels,
        datasets: [{
          label: `Total ventas ${currentYear.value}`,
          data: values,
          tension: 0.25,
          fill: false
        }]
      },
      options: {
        responsive: true,
        plugins: { legend: { display: true } },
        scales: { y: { beginAtZero: true } }
      }
    })
  } catch (e) {
    console.error('Error cargando ventas mensuales', e)
  }
}

function reloadPeriod() {
  // Hook del selector de período: por ahora usamos año y hoy.
  if (selectedPeriod.value === 'today') {
    loadResumen()
  } else if (selectedPeriod.value === 'month' || selectedPeriod.value === 'year') {
    loadVentasMensuales()
  }
}

onMounted(async () => {
  buildYears()
  await Promise.all([
    loadResumen(),
    loadBajoStock(),
    loadTopProductos(),
    loadVentasMensuales()
  ])
})
</script>

<style scoped>
/* Conservé íntegra tu hoja de estilos */
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
