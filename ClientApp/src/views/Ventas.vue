<template>
  <div class="page-container">
    <div class="page-header">
      <h1>Gestión de Ventas</h1>
      <button @click="$router.push('/ventas/nueva')" class="btn btn-primary">+ Nueva Venta</button>
    </div>

    <div class="filters-bar">
      <div class="search-input-wrapper">
        <span class="search-icon">🔍</span>
        <input v-model="searchQuery" type="text" placeholder="Buscar por factura, cliente..." class="search-input" />
      </div>
      <select v-model="filterCanal" class="filter-select">
        <option value="all">Canal: Todos</option>
        <option value="Fisico">Físico</option>
        <option value="Facebook">Facebook</option>
        <option value="WhatsApp">WhatsApp</option>
      </select>
      <select v-model="filterEstado" class="filter-select">
        <option value="all">Estado: Todos</option>
        <option value="Completada">Completada</option>
        <option value="Pendiente">Pendiente</option>
        <option value="Cancelada">Cancelada</option>
      </select>
    </div>

    <div class="table-container">
      <table class="data-table">
        <thead>
          <tr>
            <th>FACTURA</th>
            <th>FECHA</th>
            <th>CLIENTE</th>
            <th>CANAL</th>
            <th>PRODUCTOS</th>
            <th>TOTAL</th>
            <th>ESTADO</th>
            <th>ACCIONES</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="venta in filteredVentas" :key="venta.id">
            <td><strong>{{ venta.factura }}</strong></td>
            <td>{{ venta.fecha }}</td>
            <td>{{ venta.cliente }}</td>
            <td><span :class="['badge', `badge-${venta.canal.toLowerCase()}`]">{{ venta.canal }}</span></td>
            <td>{{ venta.productos }}</td>
            <td><strong>Q {{ venta.total }}</strong></td>
            <td><span :class="['status', `status-${venta.estado.toLowerCase()}`]">{{ venta.estado }}</span></td>
            <td>
              <button @click="viewVenta(venta)" class="btn-icon" title="Ver detalle">👁️</button>
              <button @click="printVenta(venta)" class="btn-icon" title="Imprimir">🖨️</button>
              <button v-if="venta.estado !== 'Cancelada'" @click="cancelVenta(venta.id)" class="btn-icon" title="Cancelar">🗑️</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="pagination-container">
      <span class="more-items">... más ventas</span>
      <div class="pagination-info">
        <span>Páginas:</span>
        <button v-for="page in 3" :key="page" :class="['pagination-btn', { active: page === 1 }]">{{ page }}</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useToast } from 'vue-toastification'

const toast = useToast()
const searchQuery = ref('')
const filterCanal = ref('all')
const filterEstado = ref('all')

const ventas = ref([
  { id: 1, factura: 'FAC-001', fecha: '04/11/2025', cliente: 'María González', canal: 'Fisico', productos: '3 items', total: '1,250.00', estado: 'Completada' },
  { id: 2, factura: 'FAC-002', fecha: '04/11/2025', cliente: 'Ana Pérez', canal: 'WhatsApp', productos: '2 items', total: '850.00', estado: 'Completada' },
  { id: 3, factura: 'FAC-003', fecha: '03/11/2025', cliente: 'Carmen Hernández', canal: 'Facebook', productos: '5 items', total: '2,100.00', estado: 'Pendiente' },
  { id: 4, factura: 'FAC-004', fecha: '03/11/2025', cliente: 'Cliente General', canal: 'Fisico', productos: '1 item', total: '400.00', estado: 'Completada' },
  { id: 5, factura: 'FAC-005', fecha: '02/11/2025', cliente: 'Chimaltenango', canal: 'WhatsApp', productos: '4 items', total: '1,800.00', estado: 'Cancelada' }
])

const filteredVentas = computed(() => {
  let filtered = ventas.value
  if (searchQuery.value) {
    filtered = filtered.filter(v => 
      v.factura.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      v.cliente.toLowerCase().includes(searchQuery.value.toLowerCase())
    )
  }
  if (filterCanal.value !== 'all') {
    filtered = filtered.filter(v => v.canal === filterCanal.value)
  }
  if (filterEstado.value !== 'all') {
    filtered = filtered.filter(v => v.estado === filterEstado.value)
  }
  return filtered
})

const viewVenta = (venta) => {
  toast.info(`Ver detalle de ${venta.factura}`)
}

const printVenta = (venta) => {
  toast.info(`Imprimiendo ${venta.factura}`)
}

const cancelVenta = (id) => {
  if (confirm('¿Cancelar esta venta?')) {
    const venta = ventas.value.find(v => v.id === id)
    if (venta) {
      venta.estado = 'Cancelada'
      toast.success('Venta cancelada')
    }
  }
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
  min-width: 180px;
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

.badge {
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: var(--font-weight-medium);
}

.badge-fisico {
  background: #e3f2fd;
  color: #1976d2;
}

.badge-facebook {
  background: #e8f5e9;
  color: #388e3c;
}

.badge-whatsapp {
  background: #fff3e0;
  color: #f57c00;
}

.status {
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: var(--font-weight-medium);
}

.status-completada {
  background: var(--color-success-light);
  color: var(--color-success);
}

.status-pendiente {
  background: var(--color-warning-light);
  color: var(--color-warning);
}

.status-cancelada {
  background: var(--color-error-light);
  color: var(--color-error);
}

.btn-icon {
  background: transparent;
  border: none;
  cursor: pointer;
  padding: 4px 8px;
  font-size: 18px;
}

.pagination-container {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.pagination-info {
  display: flex;
  gap: 8px;
  align-items: center;
}

.pagination-btn {
  padding: 8px 16px;
  border: 2px solid var(--color-border);
  background: white;
  border-radius: var(--border-radius-md);
  cursor: pointer;
}

.pagination-btn.active {
  background: var(--color-primary);
  color: white;
  border-color: var(--color-primary);
}
</style>