<template>
  <div class="page-container">
    <div class="page-header">
      <h1>Gestión de Clientes</h1>
      <button @click="openModal()" class="btn btn-primary">+ Añadir Cliente</button>
    </div>

    <!-- Búsqueda -->
    <div class="search-bar">
      <div class="search-input-wrapper">
        <span class="search-icon">🔍</span>
        <input 
          v-model="searchQuery" 
          type="text" 
          placeholder="Buscar cliente por nombre o contacto..." 
          class="search-input"
          @input="filtrarClientes"
        />
      </div>
    </div>

    <!-- Estado de carga -->
    <div v-if="loading" class="loading-state">
      <div class="spinner"></div>
      <p>Cargando clientes...</p>
    </div>

    <!-- Tabla de clientes -->
    <div v-else class="table-container">
      <table class="data-table">
        <thead>
          <tr>
            <th>NOMBRE COMPLETO</th>
            <th>CONTACTO</th>
            <th>UBICACIÓN</th>
            <th>EMAIL</th>
            <th>ACCIONES</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="cliente in filteredClientes" :key="cliente.id">
            <td>{{ cliente.nombre }}</td>
            <td>{{ cliente.telefono }}</td>
            <td>{{ cliente.direccion }}</td>
            <td>{{ cliente.email }}</td>
            <td>
              <button @click="openModal(cliente)" class="btn-action" title="Editar">✏️</button>
              <button @click="deleteCliente(cliente.id)" class="btn-action" title="Eliminar">🗑️</button>
            </td>
          </tr>
        </tbody>
      </table>

      <!-- Sin resultados -->
      <div v-if="filteredClientes.length === 0" class="no-data">
        <p>No se encontraron clientes</p>
      </div>
    </div>

    <!-- Paginación -->
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
          <h2>{{ isEditing ? 'Editar Cliente' : 'Registrar Nuevo Cliente' }}</h2>
          <button @click="closeModal" class="btn-close">✕</button>
        </div>
        <form @submit.prevent="saveCliente" class="modal-form">
          <div class="form-group">
            <label>Nombre Completo*</label>
            <input v-model="formData.nombre" type="text" required placeholder="Ej: María González" />
          </div>
          <div class="form-group">
            <label>Teléfono*</label>
            <input v-model="formData.telefono" type="tel" required placeholder="Ej: 555-1234" />
          </div>
          <div class="form-group">
            <label>Email</label>
            <input v-model="formData.email" type="email" placeholder="cliente@ejemplo.com" />
          </div>
          <div class="form-group">
            <label>Dirección</label>
            <input v-model="formData.direccion" type="text" placeholder="Ej: Zona 1, Guatemala" />
          </div>
          
          <div v-if="formError" class="error-message">
            {{ formError }}
          </div>

          <div class="modal-actions">
            <button type="submit" class="btn btn-primary" :disabled="loadingForm">
              {{ loadingForm ? 'Guardando...' : (isEditing ? 'Actualizar' : 'Guardar Cliente') }}
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

// Estado
const clientes = ref([])
const filteredClientes = ref([])
const loading = ref(false)
const loadingForm = ref(false)
const showModal = ref(false)
const isEditing = ref(false)
const searchQuery = ref('')
const currentPage = ref(1)
const itemsPerPage = 10
const formError = ref('')

// Formulario
const formData = ref({
  id: null,
  nombre: '',
  telefono: '',
  email: '',
  direccion: ''
})

// Computed
const totalPages = computed(() => Math.ceil(filteredClientes.value.length / itemsPerPage))

const paginatedClientes = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage
  const end = start + itemsPerPage
  return filteredClientes.value.slice(start, end)
})

// Métodos
const cargarClientes = async () => {
  loading.value = true
  try {
    const response = await api.get('/Clientes')
    clientes.value = response.data.data || response.data
    filtrarClientes()
  } catch (error) {
    console.error('Error al cargar clientes:', error)
    toast.error('Error al cargar clientes')
  } finally {
    loading.value = false
  }
}

const filtrarClientes = () => {
  if (!searchQuery.value) {
    filteredClientes.value = clientes.value
  } else {
    const query = searchQuery.value.toLowerCase()
    filteredClientes.value = clientes.value.filter(c =>
      c.nombre.toLowerCase().includes(query) ||
      c.telefono.includes(query) ||
      (c.email && c.email.toLowerCase().includes(query))
    )
  }
  currentPage.value = 1
}

const openModal = (cliente = null) => {
  formError.value = ''
  if (cliente) {
    isEditing.value = true
    formData.value = { ...cliente }
  } else {
    isEditing.value = false
    formData.value = { id: null, nombre: '', telefono: '', email: '', direccion: '' }
  }
  showModal.value = true
}

const closeModal = () => {
  showModal.value = false
  formData.value = { id: null, nombre: '', telefono: '', email: '', direccion: '' }
}

const saveCliente = async () => {
  loadingForm.value = true
  formError.value = ''
  
  try {
    if (isEditing.value) {
      const response = await api.put(`/Clientes/${formData.value.id}`, {
        nombre: formData.value.nombre,
        telefono: formData.value.telefono,
        email: formData.value.email,
        direccion: formData.value.direccion
      })
      
      if (response.data.success) {
        toast.success('Cliente actualizado exitosamente')
        cargarClientes()
        closeModal()
      }
    } else {
      const response = await api.post('/Clientes', {
        nombre: formData.value.nombre,
        telefono: formData.value.telefono,
        email: formData.value.email,
        direccion: formData.value.direccion
      })
      
      if (response.data.success) {
        toast.success('Cliente creado exitosamente')
        cargarClientes()
        closeModal()
      }
    }
  } catch (error) {
    console.error('Error al guardar cliente:', error)
    formError.value = error.response?.data?.message || 'Error al guardar cliente'
    toast.error(formError.value)
  } finally {
    loadingForm.value = false
  }
}

const deleteCliente = async (id) => {
  if (!confirm('¿Estás seguro de que deseas eliminar este cliente?')) {
    return
  }
  
  try {
    const response = await api.delete(`/Clientes/${id}`)
    if (response.data.success) {
      toast.success('Cliente eliminado exitosamente')
      cargarClientes()
    }
  } catch (error) {
    console.error('Error al eliminar cliente:', error)
    toast.error('Error al eliminar cliente')
  }
}

const previousPage = () => {
  if (currentPage.value > 1) currentPage.value--
}

const nextPage = () => {
  if (currentPage.value < totalPages.value) currentPage.value++
}

// Lifecycle
onMounted(() => {
  cargarClientes()
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
}

.search-input {
  width: 100%;
  padding: 14px 16px 14px 48px;
  border: 2px solid var(--color-border);
  border-radius: var(--border-radius-md);
  font-size: 15px;
  background: white;
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

.btn-action {
  background: transparent;
  border: none;
  cursor: pointer;
  padding: 4px 8px;
  margin: 0 4px;
  font-size: 16px;
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
  transition: all 0.3s;
}

.pagination-btn:hover:not(:disabled) {
  background: var(--color-primary);
  color: white;
  border-color: var(--color-primary);
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
  max-width: 600px;
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

.form-group {
  margin-bottom: 20px;
}

.form-group label {
  display: block;
  margin-bottom: 8px;
  font-weight: var(--font-weight-medium);
}

.form-group input {
  width: 100%;
  padding: 12px 16px;
  border: 2px solid var(--color-border);
  border-radius: var(--border-radius-md);
  font-size: 15px;
}

.form-group input:focus {
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