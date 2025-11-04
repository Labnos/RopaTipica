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
          <option value="custom">Personalizado</option>
        </select>
        <button @click="exportReport" class="btn btn-secondary">📥 Exportar PDF</button>
      </div>
    </div>

    <!-- Resumen General -->
    <div class="summary-section">
      <h2 class="section-title">Resumen General</h2>
      <div class="stats-grid">
        <div class="stat-card">
          <div class="stat-icon" style="background: var(--color-primary)">💰</div>
          <div class="stat-info">
            <span class="stat-label">Ingresos Totales</span>
            <span class="stat-value">Q 45,250.00</span>
            <span class="stat-change positive">+12.5% vs período anterior</span>
          </div>
        </div>
        <div class="stat-card">
          <div class="stat-icon" style="background: var(--color-success)">🛒</div>
          <div class="stat-info">
            <span class="stat-label">Total Ventas</span>
            <span class="stat-value">156</span>
            <span class="stat-change positive">+8.3% vs período anterior</span>
          </div>
        </div>
        <div class="stat-card">
          <div class="stat-icon" style="background: var(--color-warning)">📦</div>
          <div class="stat-info">
            <span class="stat-label">Productos Vendidos</span>
            <span class="stat-value">342</span>
            <span class="stat-change neutral">0% vs período anterior</span>
          </div>
        </div>
        <div class="stat-card">
          <div class="stat-icon" style="background: var(--color-info)">👥</div>
          <div class="stat-info">
            <span class="stat-label">Nuevos Clientes</span>
            <span class="stat-value">24</span>
            <span class="stat-change positive">+15.2% vs período anterior</span>
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
          <button @click="generateReport('ventas')" class="btn btn-primary btn-sm">Generar Reporte</button>
        </div>

        <div class="report-card">
          <div class="report-icon">🎨</div>
          <h3>Productos Más Vendidos</h3>
          <p>Top 10 productos con mejor rendimiento</p>
          <button @click="generateReport('productos')" class="btn btn-primary btn-sm">Generar Reporte</button>
        </div>

        <div class="report-card">
          <div class="report-icon">👥</div>
          <h3>Clientes Frecuentes</h3>
          <p>Análisis de clientes con mayor frecuencia de compra</p>
          <button @click="generateReport('clientes')" class="btn btn-primary btn-sm">Generar Reporte</button>
        </div>

        <div class="report-card">
          <div class="report-icon">📱</div>
          <h3>Ventas por Canal</h3>
          <p>Distribución de ventas: Físico, Facebook, WhatsApp</p>
          <button @click="generateReport('canales')" class="btn btn-primary btn-sm">Generar Reporte</button>
        </div>

        <div class="report-card">
          <div class="report-icon">📦</div>
          <h3>Inventario Bajo Stock</h3>
          <p>Productos que requieren reabastecimiento</p>
          <button @click="generateReport('stock')" class="btn btn-primary btn-sm">Generar Reporte</button>
        </div>

        <div class="report-card">
          <div class="report-icon">💵</div>
          <h3>Reporte Financiero</h3>
          <p>Ingresos, egresos y utilidades del período</p>
          <button @click="generateReport('financiero')" class="btn btn-primary btn-sm">Generar Reporte</button>
        </div>
      </div>
    </div>

    <!-- Tabla de últimos reportes generados -->
    <div class="recent-reports">
      <h2 class="section-title">Reportes Recientes</h2>
      <div class="table-container">
        <table class="data-table">
          <thead>
            <tr>
              <th>TIPO DE REPORTE</th>
              <th>PERÍODO</th>
              <th>GENERADO POR</th>
              <th>FECHA GENERACIÓN</th>
              <th>ACCIONES</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="report in recentReports" :key="report.id">
              <td><strong>{{ report.tipo }}</strong></td>
              <td>{{ report.periodo }}</td>
              <td>{{ report.generadoPor }}</td>
              <td>{{ report.fecha }}</td>
              <td>
                <button @click="downloadReport(report)" class="btn-icon" title="Descargar">📥</button>
                <button @click="viewReport(report)" class="btn-icon" title="Ver">👁️</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useToast } from 'vue-toastification'

const toast = useToast()
const selectedPeriod = ref('month')

const recentReports = ref([
  { id: 1, tipo: 'Ventas por Período', periodo: 'Octubre 2025', generadoPor: 'Administrador', fecha: '01/11/2025 10:30' },
  { id: 2, tipo: 'Productos Más Vendidos', periodo: 'Octubre 2025', generadoPor: 'Carlos Mendoza', fecha: '31/10/2025 15:20' },
  { id: 3, tipo: 'Inventario Bajo Stock', periodo: 'Actual', generadoPor: 'María López', fecha: '30/10/2025 09:15' }
])

const generateReport = (tipo) => {
  toast.success(`Generando reporte de ${tipo}...`)
}

const exportReport = () => {
  toast.info('Exportando reporte a PDF...')
}

const downloadReport = (report) => {
  toast.success(`Descargando ${report.tipo}`)
}

const viewReport = (report) => {
  toast.info(`Visualizando ${report.tipo}`)
}
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
}

.period-select {
  padding: 12px 16px;
  border: 2px solid var(--color-border);
  border-radius: var(--border-radius-md);
  background: white;
  font-size: 14px;
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

.btn-icon {
  background: transparent;
  border: none;
  cursor: pointer;
  padding: 4px 8px;
  font-size: 18px;
}

.btn-sm {
  padding: 8px 16px;
  font-size: 13px;
}
</style>