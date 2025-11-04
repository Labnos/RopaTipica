<template>
  <div class="page-container">
    <!-- Header -->
    <div class="page-header">
      <h1>Gestión de Inventario</h1>
      <div class="header-right">
        <div class="date-selector">
          <span>Hoy</span>
        </div>
        <button @click="openModal()" class="btn btn-primary">
          + Añadir Producto
        </button>
      </div>
    </div>

    <!-- Filtros -->
    <div class="filters-bar">
      <div class="search-input-wrapper">
        <span class="search-icon">🔍</span>
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Buscar por nombre, proveedor..."
          class="search-input"
        />
      </div>

      <select v-model="filterDisponibilidad" class="filter-select">
        <option value="all">Disponibilidad: Todas</option>
        <option value="disponible">Disponible</option>
        <option value="bajo">Bajo Stock</option>
        <option value="agotado">Agotado</option>
      </select>

      <select v-model="filterEstado" class="filter-select">
        <option value="all">Estado: Todos</option>
        <option value="activo">Activo</option>
        <option value="inactivo">Inactivo</option>
      </select>
    </div>

    <!-- Tabla de productos -->
    <div class="table-container">
      <table class="data-table">
        <thead>
          <tr>
            <th>PRODUCTO</th>
            <th>TIPO</th>
            <th>PROVEEDOR</th>
            <th>SUCOR ADQ.</th>
            <th>PRECIO VENTA</th>
            <th>STOCK</th>
            <th>ACCIONES</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="producto in filteredProductos" :key="producto.id" :class="{ 'row-highlight': producto.destacado }">
            <td>{{ producto.nombre }}</td>
            <td>{{ producto.tipo }}</td>
            <td>{{ producto.proveedor }}</td>
            <td>{{ producto.sucursal }}</td>
            <td>Q {{ producto.precioVenta }}</td>
            <td>{{ producto.stock }}</td>
            <td>
              <button @click="openModal(producto)" class="btn-icon" title="Editar">
                Editar 📝
              </button>
              <button @click="deleteProducto(producto.id)" class="btn-icon" title="Eliminar">
                🗑️
              </button>
            </td>
          </tr>
          <tr v-if="filteredProductos.length === 0">
            <td colspan="7" class="no-data">No se encontraron productos</td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Paginación -->
    <div class="pagination-container">
      <span class="more-items">... más clientes</span>
      <div class="pagination">
        <button
          v-for="page in totalPages"
          :key="page"
          @click="currentPage = page"
          :class="['pagination-btn', { active: currentPage === page }]"
        >
          {{ page }}
        </button>
      </div>
      <div class="pagination-info">
        <span>Páginas: {{ currentPage }}</span>
        <button
          @click="currentPage++"
          :disabled="currentPage >= totalPages"
          class="pagination-nav"
        >
          {{ currentPage }}
        </button>
        <button
          @click="currentPage++"
          :disabled="currentPage >= totalPages"
          class="pagination-nav"
        >
          {{ currentPage + 1 }}
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
              <label>Proveedor*</label>
              <input v-model="formData.proveedor" type="text" required />
            </div>
            <div class="form-group">
              <label>Sucursal*</label>
              <input v-model="formData.sucursal" type="text" required />
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Precio Compra*</label>
              <input v-model="formData.precioCompra" type="number" step="0.01" required />
            </div>
            <div class="form-group">
              <label>Precio Venta*</label>
              <input v-model="formData.precioVenta" type="number" step="0.01" required />
            </div>
          </div>

          <div class="form-group">
            <label>Stock Inicial*</label>
            <input v-model="formData.stock" type="number" required />
          </div>

          <div class="modal-actions">
            <button type="submit" class="btn btn-primary">
              {{ isEditing ? 'Actualizar' : 'Guardar Producto' }}
            </button>
            <button type="button" @click="closeModal" class="btn btn-secondary">
              Cancelar
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useToast } from 'vue-toastification'

const toast = useToast()

const searchQuery = ref('')
const filterDisponibilidad = ref('all')
const filterEstado = ref('all')
const showModal = ref(false)
const isEditing = ref(false)
const currentPage = ref(1)
const itemsPerPage = 10

const formData = ref({
  id: null,
  nombre: '',
  tipo: 'Corte',
  proveedor: '',
  sucursal: '',
  precioCompra: '',
  precioVenta: '',
  stock: ''
})

const productos = ref([
  { id: 1, nombre: 'Corte Quiché', tipo: 'Corte', proveedor: 'Angelina', sucursal: 'Tienda 1', precioVenta: '1,000.00', stock: 15, destacado: true },
  { id: 2, nombre: 'Guipil Comalapa', tipo: 'Guipil', proveedor: 'Sandra', sucursal: 'Tienda 2', precioVenta: '1,500.00', stock: 10, destacado: false },
  { id: 3, nombre: 'Guipil', tipo: 'Guipil', proveedor: 'Sandra', sucursal: 'Tienda 1', precioVenta: '400.00', stock: 25, destacado: false },
  { id: 4, nombre: 'Blusa Quiché', tipo: 'Guipil', proveedor: 'Santos', sucursal: 'Tienda 3', precioVenta: '350.00', stock: 5, destacado: false },
  { id: 5, nombre: 'Perraje Totonica...', tipo: 'Perraje', proveedor: 'Santos', sucursal: 'Tienda 3', precioVenta: '1,750.00', stock: 5500.0, destacado: false }
])

const filteredProductos = computed(() => {
  let filtered = productos.value
  if (searchQuery.value) {
    filtered = filtered.filter(p =>
      p.nombre.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      p.proveedor.toLowerCase().includes(searchQuery.value.toLowerCase())
    )
  }
  return filtered.slice((currentPage.value - 1) * itemsPerPage, currentPage.value * itemsPerPage)
})

const totalPages = computed(() => Math.ceil(productos.value.length / itemsPerPage))

const openModal = (producto = null) => {
  if (producto) {
    isEditing.value = true
    formData.value = { ...producto }
  } else {
    isEditing.value = false
    formData.value = { id: null, nombre: '', tipo: 'Corte', proveedor: '', sucursal: '', precioCompra: '', precioVenta: '', stock: '' }
  }
  showModal.value = true
}

const closeModal = () => {
  showModal.value = false
}

const saveProducto = () => {
  if (isEditing.value) {
    const index = productos.value.findIndex(p => p.id === formData.value.id)
    if (index !== -1) productos.value[index] = { ...formData.value }
    toast.success('Producto actualizado')
  } else {
    productos.value.push({ ...formData.value, id: productos.value.length + 1 })
    toast.success('Producto creado')
  }
  closeModal()
}

const deleteProducto = (id) => {
  if (confirm('¿Eliminar producto?')) {
    productos.value = productos.value.filter(p => p.id !== id)
    toast.success('Producto eliminado')
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

.header-right {
  display: flex;
  gap: 16px;
  align-items: center;
}

.date-selector {
  padding: 10px 20px;
  background: white;
  border: 2px solid var(--color-border);
  border-radius: var(--border-radius-md);
  font-weight: var(--font-weight-medium);
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
  font-size: 15px;
  background: white;
  min-width: 200px;
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
  letter-spacing: 0.5px;
  border-bottom: 2px solid var(--color-border);
}

.data-table td {
  padding: 16px;
  font-size: 14px;
  border-bottom: 1px solid var(--color-background);
}

.row-highlight {
  background: var(--color-primary) !important;
  color: white;
}

.row-highlight td {
  color: white;
}

.btn-icon {
  background: transparent;
  border: none;
  cursor: pointer;
  padding: 4px 8px;
  margin: 0 4px;
  font-size: 14px;
  transition: transform 0.2s;
}

.btn-icon:hover {
  transform: scale(1.1);
}

.pagination-container {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.more-items {
  font-size: 14px;
  color: var(--color-text-muted);
}

.pagination {
  display: flex;
  gap: 8px;
}

.pagination-btn {
  padding: 8px 16px;
  border: 2px solid var(--color-border);
  background: white;
  border-radius: var(--border-radius-md);
  cursor: pointer;
  font-weight: var(--font-weight-medium);
}

.pagination-btn.active {
  background: var(--color-primary);
  color: white;
  border-color: var(--color-primary);
}

.pagination-info {
  display: flex;
  gap: 8px;
  align-items: center;
}

.pagination-nav {
  padding: 8px 12px;
  border: 2px solid var(--color-border);
  background: white;
  border-radius: var(--border-radius-md);
  cursor: pointer;
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
  font-size: 14px;
}

.form-group input,
.form-group select {
  width: 100%;
  padding: 12px 16px;
  border: 2px solid var(--color-border);
  border-radius: var(--border-radius-md);
  font-size: 15px;
}

.modal-actions {
  display: flex;
  gap: 12px;
  margin-top: 24px;
}

.modal-actions .btn {
  flex: 1;
}

.no-data {
  text-align: center;
  padding: 40px;
  color: var(--color-text-muted);
}
</style>