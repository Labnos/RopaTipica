<template>
  <div class="page-container">
    <div class="page-header">
      <h1>Gestión de Ventas</h1>
      <button @click="$router.push('/ventas/nueva')" class="btn btn-primary">+ Nueva Venta</button>
    </div>

    <div class="filters-bar">
      <div class="search-input-wrapper">
        <span class="search-icon">🔍</span>
        <input 
          v-model="searchQuery" 
          type="text" 
          placeholder="Buscar por factura, cliente..." 
          class="search-input"
          @input="filtrarVentas"
        />
      </div>

      <select v-model="filterCanal" class="filter-select" @change="filtrarVentas">
        <option value="">Canal: Todos</option>
        <option value="Fisico">Físico</option>
        <option value="Facebook">Facebook</option>
        <option value="WhatsApp">WhatsApp</option>
      </select>

      <select v-model="filterEstado" class="filter-select" @change="filtrarVentas">
        <option value="">Estado: Todos</option>
        <option value="Completada">Completada</option>
        <option value="Pendiente">Pendiente</option>
        <option value="Cancelada">Cancelada</option>
      </select>
    </div>

    <div v-if="loading" class="loading-state">
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
            <th>ESTADO PAGO</th>
            <th>ESTADO</th>
            <th>ACCIONES</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="venta in paginatedVentas" :key="venta.id">
            <td><strong>{{ venta.numeroFactura }}</strong></td>
            <td>{{ formatearFecha(venta.fecha) }}</td>
            <td>{{ venta.clienteNombre }}</td>
            <td><span :class="['badge-canal', `badge-${venta.canal.toLowerCase()}`]">{{ venta.canal }}</span></td>
            <td><strong>Q {{ venta.total.toFixed(2) }}</strong></td>
            <td>
              <span :class="['status-pago', venta.estadoPago === 'Pagado' ? 'pagado' : 'pendiente']">
                {{ venta.estadoPago }}
              </span>
            </td>
            <td>
              <span :class="['status', `status-${venta.estadoVenta.toLowerCase()}`]">
                {{ venta.estadoVenta }}
              </span>
            </td>
            <td>
              <button @click="viewVenta(venta)" class="btn-icon" title="Ver">👁️</button>
              <button @click="printVenta(venta)" class="btn-icon" title="Imprimir">🖨️</button>
              <button 
                v-if="venta.estadoVenta !== 'Cancelada'" 
                @click="cancelVenta(venta.id)" 
                class="btn-icon" 
                title="Cancelar"
              >
                🗑️
              </button>
            </td>
          </tr>
        </tbody>
      </table>

      <div v-if="filteredVentas.length === 0" class="no-data">
        <p>No se encontraron ventas</p>
      </div>
    </div>

    <div v-if="totalPages > 1" class="pagination-container">
      <div class="pagination-info">
        <button 
          @click="previousPage" 
          :disabled="currentPage === 1"
          class="pagination-btn"
        >
          ❮ Anterior
        </button>
        <span>Página {{ currentPage }} de {{ totalPages }}</span>
        <button 
          @click="nextPage" 
          :disabled="currentPage === totalPages"
          class="pagination-btn"
        >
          Siguiente ❯
        </button>
      </div>
    </div>

    <!-- Modal de detalles -->
    <div v-if="showDetailModal" class="modal-overlay" @click.self="showDetailModal = false">
      <div class="modal-card">
        <div class="modal-header">
          <h2>Detalles de Venta - {{ ventaSeleccionada?.numeroFactura }}</h2>
          <button @click="showDetailModal = false" class="btn-close">✕</button>
        </div>
        <div v-if="ventaSeleccionada" class="modal-content">
          <div class="detail-section">
            <h3>Información General</h3>
            <p><strong>Fecha:</strong> {{ formatearFecha(ventaSeleccionada.fecha) }}</p>
            <p><strong>Cliente:</strong> {{ ventaSeleccionada.clienteNombre }}</p>
            <p><strong>Canal:</strong> {{ ventaSeleccionada.canal }}</p>
            <p><strong>Vendedor:</strong> {{ ventaSeleccionada.usuarioNombre }}</p>
          </div>

          <div class="detail-section">
            <h3>Productos</h3>
            <table class="details-table">
              <thead>
                <tr>
                  <th>Producto</th>
                  <th>Cantidad</th>
                  <th>Precio</th>
                  <th>Subtotal</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="detalle in ventaSeleccionada.detallesVentas" :key="detalle.id">
                  <td>{{ detalle.productoNombre }}</td>
                  <td>{{ detalle.cantidad }}</td>
                  <td>Q {{ detalle.precioUnitario.toFixed(2) }}</td>
                  <td>Q {{ (detalle.cantidad * detalle.precioUnitario).toFixed(2) }}</td>
                </tr>
              </tbody>
            </table>
          </div>

          <div class="detail-section">
            <h3>Resumen de Pago</h3>
            <p><strong>Subtotal:</strong> Q {{ ventaSeleccionada.subtotal.toFixed(2) }}</p>
            <p><strong>Descuento:</strong> Q {{ ventaSeleccionada.descuento.toFixed(2) }}</p>
            <p><strong>IVA (12%):</strong> Q {{ ventaSeleccionada.iva.toFixed(2) }}</p>
            <p class="total"><strong>Total:</strong> Q {{ ventaSeleccionada.total.toFixed(2) }}</p>
            <p><strong>Método de Pago:</strong> {{ ventaSeleccionada.metodoPago }}</p>
            <p><strong>Estado de Pago:</strong> {{ ventaSeleccionada.estadoPago }}</p>
          </div>

          <div v-if="ventaSeleccionada.observaciones" class="detail-section">
            <h3>Observaciones</h3>
            <p>{{ ventaSeleccionada.observaciones }}</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useToast } from 'vue-toastification'
import api from '../services/api'

const toast = useToast()

const ventas = ref([])
const filteredVentas = ref([])
const loading = ref(false)
const showDetailModal = ref(false)
const ventaSeleccionada = ref(null)
const searchQuery = ref('')
const filterCanal = ref('')
const filterEstado = ref('')
const currentPage = ref(1)
const itemsPerPage = 10

const totalPages = computed(() => Math.ceil(filteredVentas.value.length / itemsPerPage))

const paginatedVentas = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage
  const end = start + itemsPerPage
  return filteredVentas.value.slice(start, end)
})

const cargarVentas = async () => {
  loading.value = true
  try {
    const response = await api.get('/Ventas')
    ventas.value = response.data.data || response.data
    filtrarVentas()
  } catch (error) {
    console.error('Error al cargar ventas:', error)
    toast.error('Error al cargar ventas')
  } finally {
    loading.value = false
  }
}

const filtrarVentas = () => {
  let filtered = ventas.value

  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(v =>
      v.numeroFactura.toLowerCase().includes(query) ||
      v.clienteNombre.toLowerCase().includes(query)
    )
  }

  if (filterCanal.value) {
    filtered = filtered.filter(v => v.canal === filterCanal.value)
  }

  if (filterEstado.value) {
    filtered = filtered.filter(v => v.estadoVenta === filterEstado.value)
  }

  filteredVentas.value = filtered.sort((a, b) => new Date(b.fecha) - new Date(a.fecha))
  currentPage.value = 1
}

const formatearFecha = (fecha) => {
  if (!fecha) return 'N/A'
  return new Date(fecha).toLocaleDateString('es-GT')
}

const viewVenta = (venta) => {
  ventaSeleccionada.value = venta
  showDetailModal.value = true
}

const printVenta = (venta) => {
  toast.info(`Preparando impresión de ${venta.numeroFactura}...`)
  // Aquí irá la lógica para imprimir
}

const cancelVenta = async (id) => {
  if (!confirm('¿Estás seguro de que deseas cancelar esta venta?')) {
    return
  }

  try {
    const response = await api.post(`/Ventas/${id}/Cancelar`)
    if (response.data.success) {
      toast.success('Venta cancelada exitosamente')
      cargarVentas()
    }
  } catch (error) {
    console.error('Error al cancelar venta:', error)
    toast.error('Error al cancelar venta')
  }
}

const previousPage = () => {
  if (currentPage.value > 1) currentPage.value--
}

const nextPage = () => {
  if (currentPage.value < totalPages.value) currentPage.value++
}

onMounted(() => {
  cargarVentas()
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

.filters-bar {
  display: flex;
  gap: 16px;
  margin-bottom: 24px;
}

.search-input-wrapper {
  position: relative;
  flex: 1;
}

.search-icon {
  position: absolute;
  left: 16px;
  top: 50%;
  transform: translateY(-50%);
  font-size: 18px;
}

.search-input {
  width: 100%;
  padding: 14px 16px 14px 48px;
  border: 2px solid var(--color-border);
  border-radius: var(--border-radius-md);
  font-size: 15px;
  background: white;
}

.filter-select {
  padding: 14px 16px;
  border: 2px solid var(--color-border);
  border-radius: var(--border-radius-md);
  background: white;
  min-width: 150px;
  cursor: pointer;
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

.table-container {
  background: white;
  border: 2px solid var(--color-border);
  border-radius: var(--border-radius-lg);
  overflow: hidden;
  margin-bottom: 20px;
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

.status-pago {
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: var(--font-weight-bold);
}

.status-pago.pagado {
  background: var(--color-success-light);
  color: var(--color-success);
}

.status-pago.pendiente {
  background: var(--color-warning-light);
  color: var(--color-warning);
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

.btn-icon {
  background: transparent;
  border: none;
  cursor: pointer;
  padding: 4px 8px;
  font-size: 18px;
  margin: 0 2px;
}

.no-data {
  padding: 40px;
  text-align: center;
  color: var(--color-text-muted);
}

.pagination-container {
  display: flex;
  justify-content: center;
  margin-top: 20px;
}

.pagination-info {
  display: flex;
  gap: 12px;
  align-items: center;
}

.pagination-btn {
  padding: 8px 16px;
  border: 2px solid var(--color-border);
  background: white;
  border-radius: var(--border-radius-md);
  cursor: pointer;
  font-weight: var(--font-weight-medium);
}

.pagination-btn:hover:not(:disabled) {
  background: var(--color-primary);
  color: white;
}

.pagination-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-card {
  background: white;
  border: 3px solid var(--color-primary);
  border-radius: var(--border-radius-lg);
  max-width: 800px;
  width: 100%;
  max-height: 90vh;
  overflow-y: auto;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 24px;
  border-bottom: 2px solid var(--color-border);
}

.modal-header h2 {
  font-family: var(--font-title);
  font-size: 24px;
  color: var(--color-primary);
  margin: 0;
}

.btn-close {
  background: transparent;
  border: none;
  font-size: 28px;
  cursor: pointer;
  color: var(--color-text-muted);
}

.modal-content {
  padding: 24px;
}

.detail-section {
  margin-bottom: 24px;
  padding-bottom: 24px;
  border-bottom: 1px solid var(--color-border);
}

.detail-section h3 {
  font-family: var(--font-title);
  font-size: 16px;
  color: var(--color-primary);
  margin: 0 0 12px 0;
}

.detail-section p {
  margin: 8px 0;
  font-size: 14px;
}

.detail-section p.total {
  font-size: 18px;
  color: var(--color-primary);
  margin-top: 12px;
  padding-top: 12px;
  border-top: 2px solid var(--color-border);
}

.details-table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 12px;
}

.details-table thead {
  background: var(--color-background-secondary);
}

.details-table th {
  padding: 8px;
  text-align: left;
  font-weight: var(--font-weight-bold);
  font-size: 12px;
}

.details-table td {
  padding: 8px;
  font-size: 14px;
  border-bottom: 1px solid var(--color-border);
}
</style>