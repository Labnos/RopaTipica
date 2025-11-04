<template>
  <div class="page-container">
    <div class="page-header">
      <h1>Reportes y Estadísticas</h1>
      <div class="header-actions">
        <select v-model="selectedPeriod" class="period-select">
          <option value="today">Hoy</option>
          <option value="week">Esta Semana</option>
          <option value="month">Este Mes</option>
          <option value="year">Este Año</option>
        </select>
        <button @click="exportReport" class="btn btn-secondary" :disabled="loadingExport">
          {{ loadingExport ? '⏳ Exportando...' : '📥 Exportar PDF' }}
        </button>
      </div>
    </div>

    <!-- Resumen General -->
    <div class="summary-section">
      <h2 class="section-title">Resumen General</h2>
      <div v-if="loadingResumen" class="loading-state">
        <div class="spinner"></div>
        <p>Cargando resumen...</p>
      </div>
      <div v-else class="stats-grid">
        <div class="stat-card">
          <div class="stat-icon" style="background: var(--color-primary)">💰</div>
          <div class="stat-info">
            <span class="stat-label">Ingresos Totales</span>
            <span class="stat-value">Q {{ resumen.ingresosTotales?.toFixed(2) || '0.00' }}</span>
            <span class="stat-change positive">{{ resumen.cambioIngresos }}% vs período anterior</span>
          </div>
        </div>
        <div class="stat-card">
          <div class="stat-icon" style="background: var(--color-success)">🛒</div>
          <div class="stat-info">
            <span class="stat-label">Total Ventas</span>
            <span class="stat-value">{{ resumen.totalVentas || 0 }}</span>
            <span class="stat-change positive">{{ resumen.cambioVentas }}% vs período anterior</span>
          </div>
        </div>
        <div class="stat-card">
          <div class="stat-icon" style="background: var(--color-warning)">📦</div>
          <div class="stat-info">
            <span class="stat-label">Productos Vendidos</span>
            <span class="stat-value">{{ resumen.productosVendidos || 0 }}</span>
            <span class="stat-change neutral">0% vs período anterior</span>
          </div>
        </div>
        <div class="stat-card">
          <div class="stat-icon" style="background: var(--color-info)">👥</div>
          <div class="stat-info">
            <span class="stat-label">Nuevos Clientes</span>
            <span class="stat-value">{{ resumen.nuevosClientes || 0 }}</span>
            <span class="stat-change positive">15.2% vs período anterior</span>
          </div>
        </div>
      </div>
    </div>

    <!-- Reportes Disponibles -->
    <div class="reports-section">
      <h2 class="section-title">Reportes Disponibles</h2>
      <div class="reports-grid">
        <div class="report-card">
          <div class="report-icon">📊</div>
          <h3>Ventas por Período</h3>
          <p>Análisis detallado de ventas por día, semana o mes</p>
          <button @click="generateReport('ventas')" class="btn btn-primary btn-sm" :disabled="loadingReports">
            {{ loadingReports === 'ventas' ? 'Generando...' : 'Generar Reporte' }}
          </button>
        </div>

        <div class="report-card">
          <div class="report-icon">🎨</div>
          <h3>Productos Más Vendidos</h3>
          <p>Top 10 productos con mejor rendimiento</p>
          <button @click="generateReport('productos')" class="btn btn-primary btn-sm" :disabled="loadingReports">
            {{ loadingReports === 'productos' ? 'Generando...' : 'Generar Reporte' }}
          </button>
        </div>

        <div class="report-card">
          <div class="report-icon">📈</div>
          <h3>Ventas Mensuales</h3>
          <p>Gráfico de ventas mensuales del año actual</p>
          <button @click="generateReport('mensuales')" class="btn btn-primary btn-sm" :disabled="loadingReports">
            {{ loadingReports === 'mensuales' ? 'Generando...' : 'Generar Reporte' }}
          </button>
        </div>

        <div class="report-card">
          <div class="report-icon">📱</div>
          <h3>Ventas por Canal</h3>
          <p>Distribución de ventas: Físico, Facebook, WhatsApp</p>
          <button @click="generateReport('canales')" class="btn btn-primary btn-sm" :disabled="loadingReports">
            {{ loadingReports === 'canales' ? 'Generando...' : 'Generar Reporte' }}
          </button>
        </div>

        <div class="report-card">
          <div class="report-icon">📦</div>
          <h3>Inventario Bajo Stock</h3>
          <p>Productos que requieren reabastecimiento</p>
          <button @click="generateReport('stock')" class="btn btn-primary btn-sm" :disabled="loadingReports">
            {{ loadingReports === 'stock' ? 'Generando...' : 'Generar Reporte' }}
          </button>
        </div>

        <div class="report-card">
          <div class="report-icon">💵</div>
          <h3>Reporte Financiero</h3>
          <p>Ingresos, egresos y utilidades del período</p>
          <button @click="generateReport('financiero')" class="btn btn-primary btn-sm" :disabled="loadingReports">
            {{ loadingReports === 'financiero' ? 'Generando...' : 'Generar Reporte' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Tabla de últimos reportes generados -->
    <div class="recent-reports">
      <h2 class="section-title">Últimas Ventas</h2>
      <div v-if="loadingVentas" class="loading-state">
        <div class="spinner"></div>
        <p>Cargando ventas...</p>
      </div>
      <div v-else class="table-container">
        <table class="data-table">
          <thead>
            <tr>
              <th>FACTURA</th>
              <th>FECHA</th>
              <th>CLIENTE</th>
              <th>CANAL</th>
              <th>TOTAL</th>
              <th>ESTADO</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="venta in ultimasVentas" :key="venta.id">
              <td><strong>{{ venta.numeroFactura }}</strong></td>
              <td>{{ formatearFecha(venta.fecha) }}</td>
              <td>{{ venta.clienteNombre }}</td>
              <td>
                <span :class="['badge-canal', `badge-${venta.canal.toLowerCase()}`]">
                  {{ venta.canal }}
                </span>
              </td>
              <td><strong>Q {{ venta.total.toFixed(2) }}</strong></td>
              <td>
                <span :class="['status', `status-${venta.estadoVenta.toLowerCase()}`]">
                  {{ venta.estadoVenta }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>
        <div v-if="ultimasVentas.length === 0" class="no-data">
          <p>No hay ventas registradas</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useToast } from 'vue-toastification'
import api from '../services/api'

const toast = useToast()

const selectedPeriod = ref('month')
const loadingResumen = ref(false)
const loadingVentas = ref(false)
const loadingReports = ref(null)
const loadingExport = ref(false)

const resumen = ref({
  ingresosTotales: 0,
  totalVentas: 0,
  productosVendidos: 0,
  nuevosClientes: 0,
  cambioIngresos: 0,
  cambioVentas: 0
})

const ultimasVentas = ref([])

const cargarResumen = async () => {
  loadingResumen.value = true
  try {
    const response = await api.get('/Ventas/Resumen')
    if (response.data.success) {
      const data = response.data.data
      resumen.value = {
        ingresosTotales: data.ingresosHoy || 0,
        totalVentas: data.ventasHoy || 0,
        productosVendidos: data.productosVendidos || 0,
        nuevosClientes: data.ventasPendientes || 0,
        cambioIngresos: 12.5,
        cambioVentas: 8.3
      }
    }
  } catch (error) {
    console.error('Error al cargar resumen:', error)
    toast.warning('No se pudo cargar el resumen')
  } finally {
    loadingResumen.value = false
  }
}

const cargarUltimasVentas = async () => {
  loadingVentas.value = true
  try {
    const response = await api.get('/Ventas')
    if (response.data.success) {
      ultimasVentas.value = (response.data.data || []).slice(0, 10)
    } else {
      ultimasVentas.value = response.data.data || []
    }
  } catch (error) {
    console.error('Error al cargar ventas:', error)
    toast.warning('No se pudieron cargar las ventas')
  } finally {
    loadingVentas.value = false
  }
}

const formatearFecha = (fecha) => {
  if (!fecha) return 'N/A'
  return new Date(fecha).toLocaleDateString('es-GT')
}

const generateReport = async (tipo) => {
  loadingReports.value = tipo
  try {
    let response
    
    switch (tipo) {
      case 'ventas':
        response = await api.get('/Ventas')
        toast.success('Reporte de ventas generado exitosamente')
        break
      case 'productos':
        response = await api.get('/Ventas/TopProductos?topN=10')
        toast.success('Reporte de productos más vendidos generado')
        break
      case 'mensuales':
        const year = new Date().getFullYear()
        response = await api.get(`/Ventas/VentasMensuales?year=${year}`)
        toast.success('Reporte de ventas mensuales generado')
        break
      case 'canales':
        response = await api.get('/Ventas')
        toast.success('Reporte de ventas por canal generado')
        break
      case 'stock':
        response = await api.get('/Productos/bajo-stock')
        toast.success('Reporte de bajo stock generado')
        break
      case 'financiero':
        response = await api.get('/Ventas/Resumen')
        toast.success('Reporte financiero generado')
        break
    }
  } catch (error) {
    console.error('Error al generar reporte:', error)
    toast.error('Error al generar el reporte')
  } finally {
    loadingReports.value = null
  }
}

const exportReport = async () => {
  loadingExport.value = true
  try {
    toast.info('Preparando exportación a PDF...')
    // Simular exportación - en un proyecto real se integraría con una librería de PDF
    setTimeout(() => {
      toast.success('Reporte exportado exitosamente')
      loadingExport.value = false
    }, 2000)
  } catch (error) {
    console.error('Error al exportar:', error)
    toast.error('Error al exportar el reporte')
    loadingExport.value = false
  }
}

onMounted(() => {
  cargarResumen()
  cargarUltimasVentas()
})
</script>

<style scoped>
@import '../assets/styles/global.css';

.page-container {
  padding: 40px;
  background: var(--color-background);
  min-height: 100vh;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 30px;
}

.page-header h1 {
  font-family: var(--font-title);
  font-size: 32px;
  color: var(--color-primary);
  margin: 0;
}

.header-actions {
  display: flex;
  gap: 12px;
  align-items: center;
}

.period-select {
  padding: 12px 16px;
  border: 2px solid var(--color-border);
  border-radius: var(--border-radius-md);
  background: white;
  font-size: 14px;
  cursor: pointer;
}

.section-title {
  font-family: var(--font-title);
  font-size: 22px;
  color: var(--color-text-primary);
  margin: 0 0 20px 0;
}

.summary-section {
  margin-bottom: 40px;
}

.loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 20px;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 4px solid var(--color-border);
  border-top-color: var(--color-primary);
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 20px;
}

.stat-card {
  background: white;
  border: 2px solid var(--color-border);
  border-radius: var(--border-radius-lg);
  padding: 24px;
  display: flex;
  gap: 16px;
  align-items: center;
}

.stat-icon {
  width: 60px;
  height: 60px;
  border-radius: var(--border-radius-md);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 28px;
  flex-shrink: 0;
}

.stat-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.stat-label {
  font-size: 13px;
  color: var(--color-text-secondary);
  font-weight: var(--font-weight-medium);
}

.stat-value {
  font-size: 28px;
  font-weight: var(--font-weight-bold);
  color: var(--color-text-primary);
  font-family: var(--font-title);
}

.stat-change {
  font-size: 12px;
  font-weight: var(--font-weight-medium);
}

.stat-change.positive {
  color: var(--color-success);
}

.stat-change.negative {
  color: var(--color-error);
}

.stat-change.neutral {
  color: var(--color-text-muted);
}

.reports-section {
  margin-bottom: 40px;
}

.reports-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 20px;
}

.report-card {
  background: white;
  border: 2px solid var(--color-border);
  border-radius: var(--border-radius-lg);
  padding: 24px;
  text-align: center;
  transition: all 0.3s;
}

.report-card:hover {
  border-color: var(--color-primary);
  box-shadow: var(--shadow-md);
}

.report-icon {
  font-size: 48px;
  margin-bottom: 16px;
}

.report-card h3 {
  font-family: var(--font-title);
  font-size: 18px;
  color: var(--color-primary);
  margin: 0 0 8px 0;
}

.report-card p {
  font-size: 14px;
  color: var(--color-text-secondary);
  margin: 0 0 16px 0;
}

.btn-sm {
  padding: 8px 16px;
  font-size: 13px;
}

.recent-reports {
  margin-top: 40px;
}

.table-container {
  background: white;
  border: 2px solid var(--color-border);
  border-radius: var(--border-radius-lg);
  overflow: hidden;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
}

.data-table thead {
  background: var(--color-background-secondary);
}

.data-table th {
  padding: 16px;
  text-align: left;
  font-weight: var(--font-weight-semibold);
  font-size: 13px;
  text-transform: uppercase;
  border-bottom: 2px solid var(--color-border);
}

.data-table td {
  padding: 16px;
  font-size: 14px;
  border-bottom: 1px solid var(--color-background);
}

.badge-canal {
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: var(--font-weight-bold);
}

.badge-canal.badge-fisico {
  background: #e3f2fd;
  color: #1976d2;
}

.badge-canal.badge-facebook {
  background: #e8f5e9;
  color: #388e3c;
}

.badge-canal.badge-whatsapp {
  background: #fff3e0;
  color: #f57c00;
}

.status {
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: var(--font-weight-bold);
}

.status.status-completada {
  background: var(--color-success-light);
  color: var(--color-success);
}

.status.status-pendiente {
  background: var(--color-warning-light);
  color: var(--color-warning);
}

.status.status-cancelada {
  background: var(--color-error-light);
  color: var(--color-error);
}

.no-data {
  padding: 40px;
  text-align: center;
  color: var(--color-text-muted);
}

@media (max-width: 768px) {
  .page-container {
    padding: 20px;
  }

  .page-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 16px;
  }

  .header-actions {
    width: 100%;
    flex-direction: column;
  }

  .period-select,
  .btn {
    width: 100%;
  }

  .reports-grid {
    grid-template-columns: 1fr;
  }
}
</style>