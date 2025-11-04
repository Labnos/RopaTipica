<template>
  <div class="page-container">
    <div class="page-header">
      <h1>Gestión de Usuarios</h1>
      <button @click="openModal()" class="btn btn-primary">+ Añadir Usuario</button>
    </div>

    <div class="search-bar">
      <div class="search-input-wrapper">
        <span class="search-icon">🔍</span>
        <input 
          v-model="searchQuery" 
          type="text" 
          placeholder="Buscar usuario por nombre o email..." 
          class="search-input"
          @input="filtrarUsuarios"
        />
      </div>
    </div>

    <div v-if="loading" class="loading-state">
      <div class="spinner"></div>
      <p>Cargando usuarios...</p>
    </div>

    <div v-else class="table-container">
      <table class="data-table">
        <thead>
          <tr>
            <th>NOMBRE</th>
            <th>EMAIL</th>
            <th>ROL</th>
            <th>ESTADO</th>
            <th>ÚLTIMO ACCESO</th>
            <th>ACCIONES</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="usuario in paginatedUsuarios" :key="usuario.id">
            <td><strong>{{ usuario.nombre }}</strong></td>
            <td>{{ usuario.email }}</td>
            <td><span :class="['badge', `badge-${usuario.rol.toLowerCase()}`]">{{ usuario.rol }}</span></td>
            <td>
              <span :class="['status', usuario.estado ? 'status-activo' : 'status-inactivo']">
                {{ usuario.estado ? 'Activo' : 'Inactivo' }}
              </span>
            </td>
            <td>{{ formatearFecha(usuario.ultimoAcceso) }}</td>
            <td>
              <button @click="openModal(usuario)" class="btn-icon" title="Editar">✏️</button>
              <button @click="toggleEstado(usuario)" class="btn-icon" :title="usuario.estado ? 'Desactivar' : 'Activar'">
                {{ usuario.estado ? '🔒' : '🔓' }}
              </button>
              <button @click="deleteUsuario(usuario.id)" class="btn-icon" title="Eliminar">🗑️</button>
            </td>
          </tr>
        </tbody>
      </table>

      <div v-if="filteredUsuarios.length === 0" class="no-data">
        <p>No se encontraron usuarios</p>
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
          <h2>{{ isEditing ? 'Editar Usuario' : 'Registrar Nuevo Usuario' }}</h2>
          <button @click="closeModal" class="btn-close">✕</button>
        </div>
        <form @submit.prevent="saveUsuario" class="modal-form">
          <div class="form-group">
            <label>Nombre Completo*</label>
            <input v-model="formData.nombre" type="text" required />
          </div>
          <div class="form-group">
            <label>Email*</label>
            <input v-model="formData.email" type="email" required />
          </div>
          <div v-if="!isEditing" class="form-group">
            <label>Contraseña*</label>
            <input v-model="formData.password" type="password" required minlength="8" />
          </div>
          <div class="form-group">
            <label>Rol*</label>
            <select v-model="formData.rol" required>
              <option value="Administrador">Administrador</option>
              <option value="Vendedor">Vendedor</option>
              <option value="Encargado">Encargado</option>
            </select>
          </div>
          
          <div v-if="formError" class="error-message">
            {{ formError }}
          </div>

          <div class="modal-actions">
            <button type="submit" class="btn btn-primary" :disabled="loadingForm">
              {{ loadingForm ? 'Guardando...' : (isEditing ? 'Actualizar' : 'Guardar Usuario') }}
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

const usuarios = ref([])
const filteredUsuarios = ref([])
const loading = ref(false)
const loadingForm = ref(false)
const showModal = ref(false)
const isEditing = ref(false)
const searchQuery = ref('')
const currentPage = ref(1)
const itemsPerPage = 10
const formError = ref('')

const formData = ref({
  id: null,
  nombre: '',
  email: '',
  password: '',
  rol: 'Vendedor',
  estado: true
})

const totalPages = computed(() => Math.ceil(filteredUsuarios.value.length / itemsPerPage))

const paginatedUsuarios = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage
  const end = start + itemsPerPage
  return filteredUsuarios.value.slice(start, end)
})

const cargarUsuarios = async () => {
  loading.value = true
  try {
    const response = await api.get('/Users')
    usuarios.value = response.data.data || response.data
    filtrarUsuarios()
  } catch (error) {
    console.error('Error al cargar usuarios:', error)
    toast.error('Error al cargar usuarios')
  } finally {
    loading.value = false
  }
}

const filtrarUsuarios = () => {
  if (!searchQuery.value) {
    filteredUsuarios.value = usuarios.value
  } else {
    const query = searchQuery.value.toLowerCase()
    filteredUsuarios.value = usuarios.value.filter(u =>
      u.nombre.toLowerCase().includes(query) ||
      u.email.toLowerCase().includes(query)
    )
  }
  currentPage.value = 1
}

const formatearFecha = (fecha) => {
  if (!fecha) return 'N/A'
  return new Date(fecha).toLocaleDateString('es-GT')
}

const openModal = (usuario = null) => {
  formError.value = ''
  if (usuario) {
    isEditing.value = true
    formData.value = { ...usuario, password: '' }
  } else {
    isEditing.value = false
    formData.value = { id: null, nombre: '', email: '', password: '', rol: 'Vendedor', estado: true }
  }
  showModal.value = true
}

const closeModal = () => {
  showModal.value = false
  formData.value = { id: null, nombre: '', email: '', password: '', rol: 'Vendedor', estado: true }
}

const saveUsuario = async () => {
  loadingForm.value = true
  formError.value = ''

  try {
    if (isEditing.value) {
      const response = await api.put(`/Users/${formData.value.id}`, {
        nombre: formData.value.nombre,
        email: formData.value.email,
        rol: formData.value.rol,
        estado: formData.value.estado
      })
      
      if (response.data.success) {
        toast.success('Usuario actualizado exitosamente')
        cargarUsuarios()
        closeModal()
      }
    } else {
      const response = await api.post('/Users', {
        nombre: formData.value.nombre,
        email: formData.value.email,
        password: formData.value.password,
        rol: formData.value.rol
      })
      
      if (response.data.success) {
        toast.success('Usuario creado exitosamente')
        cargarUsuarios()
        closeModal()
      }
    }
  } catch (error) {
    console.error('Error al guardar usuario:', error)
    formError.value = error.response?.data?.message || 'Error al guardar usuario'
    toast.error(formError.value)
  } finally {
    loadingForm.value = false
  }
}

const toggleEstado = async (usuario) => {
  try {
    const response = await api.put(`/Users/${usuario.id}`, {
      estado: !usuario.estado
    })
    
    if (response.data.success) {
      usuario.estado = !usuario.estado
      toast.success(`Usuario ${usuario.estado ? 'activado' : 'desactivado'}`)
    }
  } catch (error) {
    console.error('Error al cambiar estado:', error)
    toast.error('Error al cambiar estado del usuario')
  }
}

const deleteUsuario = async (id) => {
  if (!confirm('¿Estás seguro de que deseas eliminar este usuario?')) {
    return
  }

  try {
    const response = await api.delete(`/Users/${id}`)
    if (response.data.success) {
      toast.success('Usuario eliminado exitosamente')
      cargarUsuarios()
    }
  } catch (error) {
    console.error('Error al eliminar usuario:', error)
    toast.error('Error al eliminar usuario')
  }
}

const previousPage = () => {
  if (currentPage.value > 1) currentPage.value--
}

const nextPage = () => {
  if (currentPage.value < totalPages.value) currentPage.value++
}

onMounted(() => {
  cargarUsuarios()
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

.badge {
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: var(--font-weight-bold);
}

.badge-administrador {
  background: #fce4ec;
  color: #c2185b;
}

.badge-vendedor {
  background: #e3f2fd;
  color: #1976d2;
}

.badge-encargado {
  background: #fff3e0;
  color: #f57c00;
}

.status {
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: var(--font-weight-bold);
}

.status-activo {
  background: var(--color-success-light);
  color: var(--color-success);
}

.status-inactivo {
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