<template>
  <div class="page-container">
    <div class="page-header">
      <h1>Gestión de Inventario</h1>
      <button @click="openModal()" class="btn btn-primary">+ Añadir Producto</button>
    </div>

    <div class="filters-bar">
      <div class="search-input-wrapper">
        <span class="search-icon">🔍</span>
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Buscar por nombre..."
          class="search-input"
          @input="filtrarProductos"
        />
      </div>

      <select v-model="filterTipo" class="filter-select" @change="filtrarProductos">
        <option value="">Tipo: Todos</option>
        <option value="Corte">Corte</option>
        <option value="Huipil">Huipil</option>
        <option value="Blusa">Blusa</option>
        <option value="Guipil">Guipil</option>
        <option value="Perraje">Perraje</option>
      </select>
    </div>

    <div v-if="loading" class="loading-state">
      <div class="spinner"></div>
      <p>Cargando productos...</p>
    </div>

    <div v-else class="table-container">
      <table class="data-table">
        <thead>
          <tr>
            <th>PRODUCTO</th>
            <th>TIPO</th>
            <th>PROVEEDOR</th>
            <th>PRECIO VENTA</th>
            <th>STOCK</th>
            <th>ESTADO</th>
            <th>ACCIONES</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="producto in paginatedProductos" :key="producto.id">
            <td>{{ producto.nombre }}</td>
            <td>{{ producto.tipo }}</td>
            <td>{{ producto.proveedorNombre || 'N/A' }}</td>
            <td>Q {{ producto.precioVenta.toFixed(2) }}</td>
            <td>{{ producto.tipo === 'Corte' ? producto.varasDisponibles + ' varas' : producto.stock + ' units' }}</td>
            <td>
              <span :class="['badge-stock', getStockClass(producto)]">
                {{ getStockStatus(producto) }}
              </span>
            </td>
            <td>
              <button @click="openModal(producto)" class="btn-icon" title="Editar">✏️</button>
              <button @click="deleteProducto(producto.id)" class="btn-icon" title="Eliminar">🗑️</button>
            </td>
          </tr>
        </tbody>
      </table>

      <div v-if="filteredProductos.length === 0" class="no-data">
        <p>No se encontraron productos</p>
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

    <!-- Modal -->
    <div v-if="showModal" class="modal-overlay" @click.self="closeModal">
      <div class="modal-card">
        <div class="modal-header">
          <h2>{{ isEditing ? 'Editar Producto' : 'Registrar Nuevo Producto' }}</h2>
          <button @click="closeModal" class="btn-close">✕</button>
        </div>

        <form @submit.prevent="saveProducto" class="modal-form">
          <div class="form-row">
            <div class="form-group">
              <label>Nombre del Producto*</label>
              <input v-model="formData.nombre" type="text" required />
            </div>
            <div class="form-group">
              <label>Tipo*</label>
              <select v-model="formData.tipo" required>
                <option value="Corte">Corte</option>
                <option value="Huipil">Huipil</option>
                <option value="Blusa">Blusa</option>
                <option value="Guipil">Guipil</option>
                <option value="Perraje">Perraje</option>
              </select>
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Precio Compra*</label>
              <input v-model.number="formData.precioCompra" type="number" step="0.01" required />
            </div>
            <div class="form-group">
              <label>Precio Venta*</label>
              <input v-model.number="formData.precioVenta" type="number" step="0.01" required />
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>{{ formData.tipo === 'Corte' ? 'Varas Disponibles' : 'Stock' }}*</label>
              <input v-model.number="formData.stock" type="number" required />
            </div>
            <div class="form-group">
              <label>Proveedor*</label>
              <input v-model="formData.proveedorId" type="text" placeholder="ID del proveedor" />
            </div>
          </div>

          <div v-if="formError" class="error-message">
            {{ formError }}
          </div>

          <div class="modal-actions">
            <button type="submit" class="btn btn-primary" :disabled="loadingForm">
              {{ loadingForm ? 'Guardando...' : (isEditing ? 'Actualizar' : 'Guardar Producto') }}
            </button>
            <button type="button" @click="closeModal" class="btn btn-secondary" :disabled="loadingForm">
              Cancelar
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useToast } from 'vue-toastification'
import api from '../services/api'

const toast = useToast()

const productos = ref([])
const filteredProductos = ref([])
const loading = ref(false)
const loadingForm = ref(false)
const showModal = ref(false)
const isEditing = ref(false)
const searchQuery = ref('')
const filterTipo = ref('')
const currentPage = ref(1)
const itemsPerPage = 10
const formError = ref('')

const formData = ref({
  id: null,
  nombre: '',
  tipo: 'Corte',
  precioCompra: 0,
  precioVenta: 0,
  stock: 0,
  proveedorId: null
})

const totalPages = computed(() => Math.ceil(filteredProductos.value.length / itemsPerPage))

const paginatedProductos = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage
  const end = start + itemsPerPage
  return filteredProductos.value.slice(start, end)
})

const cargarProductos = async () => {
  loading.value = true
  try {
    const response = await api.get('/Productos')
    productos.value = response.data.data || response.data
    filtrarProductos()
  } catch (error) {
    console.error('Error al cargar productos:', error)
    toast.error('Error al cargar productos')
  } finally {
    loading.value = false
  }
}

const filtrarProductos = () => {
  let filtered = productos.value

  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(p =>
      p.nombre.toLowerCase().includes(query)
    )
  }

  if (filterTipo.value) {
    filtered = filtered.filter(p => p.tipo === filterTipo.value)
  }

  filteredProductos.value = filtered
  currentPage.value = 1
}

const getStockStatus = (producto) => {
  if (producto.tipo === 'Corte') {
    if (producto.varasDisponibles === 0) return 'Agotado'
    if (producto.varasDisponibles < 3) return 'Bajo Stock'
    return 'Disponible'
  } else {
    if (producto.stock === 0) return 'Agotado'
    if (producto.stock < 5) return 'Bajo Stock'
    return 'Disponible'
  }
}

const getStockClass = (producto) => {
  const status = getStockStatus(producto)
  if (status === 'Agotado') return 'agotado'
  if (status === 'Bajo Stock') return 'bajo'
  return 'disponible'
}

const openModal = (producto = null) => {
  formError.value = ''
  if (producto) {
    isEditing.value = true
    formData.value = { ...producto }
  } else {
    isEditing.value = false
    formData.value = { id: null, nombre: '', tipo: 'Corte', precioCompra: 0, precioVenta: 0, stock: 0, proveedorId: null }
  }
  showModal.value = true
}

const closeModal = () => {
  showModal.value = false
  formData.value = { id: null, nombre: '', tipo: 'Corte', precioCompra: 0, precioVenta: 0, stock: 0, proveedorId: null }
}

const saveProducto = async () => {
  loadingForm.value = true
  formError.value = ''

  try {
    if (isEditing.value) {
      const response = await api.put(`/Productos/${formData.value.id}`, {
        nombre: formData.value.nombre,
        tipo: formData.value.tipo,
        precioCompra: formData.value.precioCompra,
        precioVenta: formData.value.precioVenta,
        stock: formData.value.stock,
        proveedorId: formData.value.proveedorId
      })
      
      if (response.data.success) {
        toast.success('Producto actualizado exitosamente')
        cargarProductos()
        closeModal()
      }
    } else {
      const response = await api.post('/Productos', {
        nombre: formData.value.nombre,
        tipo: formData.value.tipo,
        precioCompra: formData.value.precioCompra,
        precioVenta: formData.value.precioVenta,
        stock: formData.value.stock,
        proveedorId: formData.value.proveedorId
      })
      
      if (response.data.success) {
        toast.success('Producto creado exitosamente')
        cargarProductos()
        closeModal()
      }
    }
  } catch (error) {
    console.error('Error al guardar producto:', error)
    formError.value = error.response?.data?.message || 'Error al guardar producto'
    toast.error(formError.value)
  } finally {
    loadingForm.value = false
  }
}

const deleteProducto = async (id) => {
  if (!confirm('¿Estás seguro de que deseas eliminar este producto?')) {
    return
  }

  try {
    const response = await api.delete(`/Productos/${id}`)
    if (response.data.success) {
      toast.success('Producto eliminado exitosamente')
      cargarProductos()
    }
  } catch (error) {
    console.error('Error al eliminar producto:', error)
    toast.error('Error al eliminar producto')
  }
}

const previousPage = () => {
  if (currentPage.value > 1) currentPage.value--
}

const nextPage = () => {
  if (currentPage.value < totalPages.value) currentPage.value++
}

onMounted(() => {
  cargarProductos()
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
  min-width: 180px;
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

.badge-stock {
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: var(--font-weight-bold);
}

.badge-stock.disponible {
  background: var(--color-success-light);
  color: var(--color-success);
}

.badge-stock.bajo {
  background: var(--color-warning-light);
  color: var(--color-warning);
}

.badge-stock.agotado {
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
  max-width: 700px;
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

.modal-form {
  padding: 24px;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.form-group {
  margin-bottom: 20px;
}

.form-group label {
  display: block;
  margin-bottom: 8px;
  font-weight: var(--font-weight-medium);
}

.form-group input,
.form-group select {
  width: 100%;
  padding: 12px 16px;
  border: 2px solid var(--color-border);
  border-radius: var(--border-radius-md);
  font-size: 15px;
}

.form-group input:focus,
.form-group select:focus {
  outline: none;
  border-color: var(--color-primary);
}

.error-message {
  background-color: var(--color-error-light);
  color: #991b1b;
  padding: 12px 16px;
  border-radius: var(--border-radius-md);
  font-size: 14px;
  margin-bottom: 16px;
  border-left: 4px solid var(--color-error);
}

.modal-actions {
  display: flex;
  gap: 12px;
  margin-top: 24px;
}

.modal-actions .btn {
  flex: 1;
}
</style>