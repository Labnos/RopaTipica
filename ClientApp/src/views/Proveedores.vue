<template>
  <div class="page-container">
    <!-- Header -->
    <div class="page-header">
      <h1>Gestión de Proveedores</h1>
      <button @click="openModal()" class="btn btn-primary">
        + Añadir Cliente
      </button>
    </div>

    <!-- Barra de búsqueda -->
    <div class="search-bar">
      <div class="search-input-wrapper">
        <span class="search-icon">🔍</span>
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Buscar proveedor por nombre..."
          class="search-input"
        />
      </div>
    </div>

    <!-- Tabla de proveedores -->
    <div class="table-container">
      <table class="data-table">
        <thead>
          <tr>
            <th>NOMBRE COMPLETO</th>
            <th>CONTACTO</th>
            <th>DIRECCIÓN</th>
            <th>ÚLTIMA COMPRA</th>
            <th>ACCIONES</th>
            <th>ACCIONES</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="proveedor in filteredProveedores" :key="proveedor.id">
            <td>{{ proveedor.nombre }}</td>
            <td>{{ proveedor.telefono }}</td>
            <td>{{ proveedor.direccion }}</td>
            <td>{{ proveedor.ultimaCompra }}</td>
            <td>
              <button @click="openModal(proveedor)" class="btn-icon" title="Ver detalles">
                👁️ ✏️
              </button>
            </td>
            <td>
              <button @click="deleteProveedor(proveedor.id)" class="btn-icon" title="Eliminar">
                🗑️
              </button>
            </td>
          </tr>
          <tr v-if="filteredProveedores.length === 0">
            <td colspan="6" class="no-data">No se encontraron proveedores</td>
          </tr>
          <tr v-if="hasMore">
            <td colspan="6" class="load-more">... más clientes ...</td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Paginación -->
    <div class="pagination">
      <span>Páginas: {{ currentPage }}</span>
      <button
        @click="previousPage"
        :disabled="currentPage === 1"
        class="pagination-btn"
      >
        ❮
      </button>
      <button class="pagination-btn active">{{ currentPage }}</button>
      <button
        @click="nextPage"
        :disabled="!hasMore"
        class="pagination-btn"
      >
        ❯
      </button>
    </div>

    <!-- Modal de registro/edición -->
    <div v-if="showModal" class="modal-overlay" @click.self="closeModal">
      <div class="modal-card">
        <div class="modal-header">
          <h2>{{ isEditing ? 'Editar Proveedor' : 'Registrar Nuevo Proveedor' }}</h2>
          <button @click="closeModal" class="btn-close">✕</button>
        </div>

        <form @submit.prevent="saveProveedor" class="modal-form">
          <div class="form-group">
            <label for="nombre">Nombre Completo*</label>
            <input
              id="nombre"
              v-model="formData.nombre"
              type="text"
              required
              placeholder="Ej: Textiles Maya S.A."
            />
          </div>

          <div class="form-group">
            <label for="telefono">Número de Teléfono*</label>
            <input
              id="telefono"
              v-model="formData.telefono"
              type="tel"
              required
              placeholder="Ej: 555-1111"
            />
          </div>

          <div class="form-group">
            <label for="direccion">Dirección</label>
            <input
              id="direccion"
              v-model="formData.direccion"
              type="text"
              placeholder="Ej: Zona 1, Guatemala"
            />
          </div>

          <div class="form-group">
            <label for="email">Email</label>
            <input
              id="email"
              v-model="formData.email"
              type="email"
              placeholder="proveedor@ejemplo.com"
            />
          </div>

          <div class="form-group">
            <label for="productos">Productos que Suministra</label>
            <textarea
              id="productos"
              v-model="formData.productos"
              rows="3"
              placeholder="Describe los productos que suministra este proveedor..."
            ></textarea>
          </div>

          <div class="modal-actions">
            <button type="submit" class="btn btn-primary">
              {{ isEditing ? 'Actualizar' : 'Guardar Cliente' }}
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

// Estado
const searchQuery = ref('')
const showModal = ref(false)
const isEditing = ref(false)
const currentPage = ref(1)
const itemsPerPage = 10

// Datos del formulario
const formData = ref({
  id: null,
  nombre: '',
  telefono: '',
  direccion: '',
  email: '',
  productos: '',
  ultimaCompra: ''
})

// Datos de ejemplo
const proveedores = ref([
  {
    id: 1,
    nombre: 'Angelina Morales',
    telefono: '555-1111',
    direccion: 'Zona 1, Guatemala',
    email: 'angelina@ejemplo.com',
    productos: 'Cortes, Huipiles',
    ultimaCompra: 'ACICIO#'
  },
  {
    id: 2,
    nombre: 'Totonicapán',
    telefono: '555-3334',
    direccion: 'Totonicapán, Guatemala',
    email: 'totoni@ejemplo.com',
    productos: 'Textiles tradicionales',
    ultimaCompra: '18-0925'
  },
  {
    id: 3,
    nombre: 'Santos Can',
    telefono: '555-4567',
    direccion: 'Chichicastenango',
    email: 'santos@ejemplo.com',
    productos: 'Fajas, Perrajes',
    ultimaCompra: '18-0925'
  },
  {
    id: 4,
    nombre: 'Sandra Pérez',
    telefono: '555-8785',
    direccion: 'Antigua Guatemala',
    email: 'sandra@ejemplo.com',
    productos: 'Blusas bordadas',
    ultimaCompra: '01-09-201'
  }
])

// Computed
const filteredProveedores = computed(() => {
  let filtered = proveedores.value

  if (searchQuery.value) {
    filtered = filtered.filter(p =>
      p.nombre.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      p.telefono.includes(searchQuery.value)
    )
  }

  const start = (currentPage.value - 1) * itemsPerPage
  const end = start + itemsPerPage
  return filtered.slice(start, end)
})

const hasMore = computed(() => {
  return proveedores.value.length > currentPage.value * itemsPerPage
})

// Métodos
const openModal = (proveedor = null) => {
  if (proveedor) {
    isEditing.value = true
    formData.value = { ...proveedor }
  } else {
    isEditing.value = false
    resetForm()
  }
  showModal.value = true
}

const closeModal = () => {
  showModal.value = false
  resetForm()
}

const resetForm = () => {
  formData.value = {
    id: null,
    nombre: '',
    telefono: '',
    direccion: '',
    email: '',
    productos: '',
    ultimaCompra: ''
  }
}

const saveProveedor = () => {
  if (isEditing.value) {
    // Actualizar proveedor existente
    const index = proveedores.value.findIndex(p => p.id === formData.value.id)
    if (index !== -1) {
      proveedores.value[index] = { ...formData.value }
      toast.success('Proveedor actualizado exitosamente')
    }
  } else {
    // Crear nuevo proveedor
    const newProveedor = {
      ...formData.value,
      id: proveedores.value.length + 1,
      ultimaCompra: 'N/A'
    }
    proveedores.value.push(newProveedor)
    toast.success('Proveedor registrado exitosamente')
  }
  closeModal()
}

const deleteProveedor = (id) => {
  if (confirm('¿Estás seguro de eliminar este proveedor?')) {
    proveedores.value = proveedores.value.filter(p => p.id !== id)
    toast.success('Proveedor eliminado')
  }
}

const previousPage = () => {
  if (currentPage.value > 1) currentPage.value--
}

const nextPage = () => {
  if (hasMore.value) currentPage.value++
}
</script>

<style scoped>
.page-container {
  padding: 40px;
  background: var(--color-background);
  min-height: 100vh;
  font-family: var(--font-body);
}

/* ===== HEADER ===== */
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

/* ===== BÚSQUEDA ===== */
.search-bar {
  margin-bottom: 24px;
}

.search-input-wrapper {
  position: relative;
  max-width: 600px;
}

.search-icon {
  position: absolute;
  left: 16px;
  top: 50%;
  transform: translateY(-50%);
  font-size: 18px;
  color: var(--color-text-muted);
}

.search-input {
  width: 100%;
  padding: 14px 16px 14px 48px;
  border: 2px solid var(--color-border);
  border-radius: var(--border-radius-md);
  font-size: 15px;
  font-family: var(--font-body);
  background: white;
}

.search-input:focus {
  outline: none;
  border-color: var(--color-primary);
}

/* ===== TABLA ===== */
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
  font-family: var(--font-body);
  font-weight: var(--font-weight-semibold);
  font-size: 13px;
  color: var(--color-text-primary);
  text-transform: uppercase;
  letter-spacing: 0.5px;
  border-bottom: 2px solid var(--color-border);
}

.data-table td {
  padding: 16px;
  font-size: 14px;
  color: var(--color-text-secondary);
  border-bottom: 1px solid var(--color-background);
}

.data-table tbody tr:hover {
  background: var(--color-background);
}

.no-data,
.load-more {
  text-align: center;
  color: var(--color-text-muted);
  font-style: italic;
  padding: 24px !important;
}

.btn-icon {
  background: transparent;
  border: none;
  cursor: pointer;
  font-size: 18px;
  padding: 4px 8px;
  transition: transform 0.2s;
}

.btn-icon:hover {
  transform: scale(1.2);
}

/* ===== PAGINACIÓN ===== */
.pagination {
  display: flex;
  align-items: center;
  gap: 12px;
  justify-content: flex-end;
  font-size: 14px;
  color: var(--color-text-secondary);
}

.pagination-btn {
  padding: 8px 12px;
  border: 1px solid var(--color-border);
  background: white;
  border-radius: var(--border-radius-md);
  cursor: pointer;
  font-family: var(--font-body);
  transition: all 0.3s;
}

.pagination-btn:hover:not(:disabled) {
  background: var(--color-background);
}

.pagination-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.pagination-btn.active {
  background: var(--color-primary);
  color: white;
  border-color: var(--color-primary);
}

/* ===== MODAL ===== */
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
  padding: 20px;
}

.modal-card {
  background: white;
  border: 3px solid var(--color-primary);
  border-radius: var(--border-radius-lg);
  max-width: 600px;
  width: 100%;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: var(--shadow-xl);
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
  padding: 0;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: color 0.3s;
}

.btn-close:hover {
  color: var(--color-error);
}

.modal-form {
  padding: 24px;
}

.form-group {
  margin-bottom: 20px;
}

.form-group label {
  display: block;
  margin-bottom: 8px;
  font-weight: var(--font-weight-medium);
  color: var(--color-text-primary);
  font-size: 14px;
}

.form-group input,
.form-group textarea {
  width: 100%;
  padding: 12px 16px;
  border: 2px solid var(--color-border);
  border-radius: var(--border-radius-md);
  font-family: var(--font-body);
  font-size: 15px;
  transition: border-color 0.3s;
}

.form-group input:focus,
.form-group textarea:focus {
  outline: none;
  border-color: var(--color-primary);
}

.form-group textarea {
  resize: vertical;
}

.modal-actions {
  display: flex;
  gap: 12px;
  margin-top: 24px;
}

.modal-actions .btn {
  flex: 1;
}

/* ===== RESPONSIVE ===== */
@media (max-width: 768px) {
  .page-container {
    padding: 20px;
  }

  .page-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 16px;
  }

  .table-container {
    overflow-x: auto;
  }

  .data-table {
    min-width: 800px;
  }
}
</style>