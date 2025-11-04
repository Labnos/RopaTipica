<template>
  <div class="page-container">
    <div class="page-header">
      <h1>Gestión de Usuarios</h1>
      <button @click="openModal()" class="btn btn-primary">+ Añadir Usuario</button>
    </div>

    <div class="search-bar">
      <div class="search-input-wrapper">
        <span class="search-icon">🔍</span>
        <input v-model="searchQuery" type="text" placeholder="Buscar usuario por nombre o email..." class="search-input" />
      </div>
    </div>

    <div class="table-container">
      <table class="data-table">
        <thead>
          <tr>
            <th>NOMBRE</th>
            <th>EMAIL</th>
            <th>ROL</th>
            <th>ESTADO</th>
            <th>ÚLTIMO ACCESO</th>
            <th>FECHA REGISTRO</th>
            <th>ACCIONES</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="usuario in filteredUsuarios" :key="usuario.id">
            <td><strong>{{ usuario.nombre }}</strong></td>
            <td>{{ usuario.email }}</td>
            <td><span :class="['badge', `badge-${usuario.rol.toLowerCase()}`]">{{ usuario.rol }}</span></td>
            <td><span :class="['status', usuario.estado ? 'status-activo' : 'status-inactivo']">{{ usuario.estado ? 'Activo' : 'Inactivo' }}</span></td>
            <td>{{ usuario.ultimoAcceso }}</td>
            <td>{{ usuario.fechaRegistro }}</td>
            <td>
              <button @click="openModal(usuario)" class="btn-icon" title="Editar">✏️</button>
              <button @click="toggleEstado(usuario)" class="btn-icon" :title="usuario.estado ? 'Desactivar' : 'Activar'">{{ usuario.estado ? '🔒' : '🔓' }}</button>
              <button @click="deleteUsuario(usuario.id)" class="btn-icon" title="Eliminar">🗑️</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="pagination-container">
      <span class="more-items">... más usuarios</span>
      <div class="pagination-info">
        <span>Páginas:</span>
        <button v-for="page in 3" :key="page" :class="['pagination-btn', { active: page === 1 }]">{{ page }}</button>
      </div>
    </div>

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
          <div class="form-group" v-if="!isEditing">
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
          <div class="form-group">
            <label>
              <input v-model="formData.estado" type="checkbox" />
              Usuario activo
            </label>
          </div>
          <div class="modal-actions">
            <button type="submit" class="btn btn-primary">{{ isEditing ? 'Actualizar' : 'Guardar Usuario' }}</button>
            <button type="button" @click="closeModal" class="btn btn-secondary">Cancelar</button>
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
const showModal = ref(false)
const isEditing = ref(false)

const formData = ref({
  id: null,
  nombre: '',
  email: '',
  password: '',
  rol: 'Vendedor',
  estado: true
})

const usuarios = ref([
  { id: 1, nombre: 'Administrador', email: 'admin@ropatipica.gt', rol: 'Administrador', estado: true, ultimoAcceso: '04/11/2025 11:30', fechaRegistro: '01/01/2025' },
  { id: 2, nombre: 'María López', email: 'maria.lopez@ropatipica.gt', rol: 'Vendedor', estado: true, ultimoAcceso: '04/11/2025 10:15', fechaRegistro: '15/01/2025' },
  { id: 3, nombre: 'Carlos Mendoza', email: 'carlos.mendoza@ropatipica.gt', rol: 'Encargado', estado: true, ultimoAcceso: '03/11/2025 16:45', fechaRegistro: '20/01/2025' },
  { id: 4, nombre: 'Ana García', email: 'ana.garcia@ropatipica.gt', rol: 'Vendedor', estado: false, ultimoAcceso: '01/11/2025 09:00', fechaRegistro: '10/02/2025' },
  { id: 5, nombre: 'Pedro Ramírez', email: 'pedro.ramirez@ropatipica.gt', rol: 'Vendedor', estado: true, ultimoAcceso: '04/11/2025 08:30', fechaRegistro: '15/02/2025' }
])

const filteredUsuarios = computed(() => {
  if (!searchQuery.value) return usuarios.value
  return usuarios.value.filter(u => 
    u.nombre.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
    u.email.toLowerCase().includes(searchQuery.value.toLowerCase())
  )
})

const openModal = (usuario = null) => {
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
}

const saveUsuario = () => {
  if (isEditing.value) {
    const index = usuarios.value.findIndex(u => u.id === formData.value.id)
    if (index !== -1) {
      usuarios.value[index] = { ...formData.value, ultimoAcceso: usuarios.value[index].ultimoAcceso, fechaRegistro: usuarios.value[index].fechaRegistro }
      toast.success('Usuario actualizado')
    }
  } else {
    usuarios.value.push({
      ...formData.value,
      id: usuarios.value.length + 1,
      ultimoAcceso: 'N/A',
      fechaRegistro: new Date().toLocaleDateString('es-GT')
    })
    toast.success('Usuario creado')
  }
  closeModal()
}

const toggleEstado = (usuario) => {
  usuario.estado = !usuario.estado
  toast.success(`Usuario ${usuario.estado ? 'activado' : 'desactivado'}`)
}

const deleteUsuario = (id) => {
  if (confirm('¿Eliminar usuario?')) {
    usuarios.value = usuarios.value.filter(u => u.id !== id)
    toast.success('Usuario eliminado')
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
  font-weight: var(--font-weight-medium);
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

.form-group input[type="text"],
.form-group input[type="email"],
.form-group input[type="password"],
.form-group select {
  width: 100%;
  padding: 12px 16px;
  border: 2px solid var(--color-border);
  border-radius: var(--border-radius-md);
  font-size: 15px;
}

.form-group input[type="checkbox"] {
  margin-right: 8px;
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