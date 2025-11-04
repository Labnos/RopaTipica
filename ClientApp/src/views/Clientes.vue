<template>
  <div class="page-container">
    <div class="page-header">
      <h1>Gestión de Clientes</h1>
      <button @click="openModal()" class="btn btn-primary">+ Añadir Cliente</button>
    </div>

    <div class="search-bar">
      <div class="search-input-wrapper">
        <span class="search-icon">🔍</span>
        <input v-model="searchQuery" type="text" placeholder="Buscar cliente por nombre o contacto..." class="search-input" />
      </div>
    </div>

    <div class="table-container">
      <table class="data-table">
        <thead>
          <tr>
            <th>NOMBRE COMPLETO</th>
            <th>CONTACTO</th>
            <th>UBICACIÓN</th>
            <th>ÚLTIMA COMPRA</th>
            <th>COMPRAS</th>
            <th>TOTACIONES</th>
            <th>ACCIONES</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="cliente in filteredClientes" :key="cliente.id">
            <td>{{ cliente.nombre }}</td>
            <td>{{ cliente.telefono }}</td>
            <td>{{ cliente.direccion }}</td>
            <td>{{ cliente.ultimaCompra }}</td>
            <td>{{ cliente.compras }}</td>
            <td>
              <button class="btn-icon">🔧 {{ cliente.totaciones }}</button>
            </td>
            <td>
              <button @click="openModal(cliente)" class="btn-action">✏️ {{ cliente.acciones }}</button>
              <button @click="deleteCliente(cliente.id)" class="btn-action">✓ 🗑️</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="pagination-container">
      <span class="more-items">... más clientes</span>
      <div class="pagination-info">
        <span>Páginas:</span>
        <button v-for="page in 3" :key="page" :class="['pagination-btn', { active: page === currentPage }]">{{ page }}</button>
      </div>
    </div>

    <div v-if="showModal" class="modal-overlay" @click.self="closeModal">
      <div class="modal-card">
        <div class="modal-header">
          <h2>Registrar Nuevo Cliente</h2>
          <button @click="closeModal" class="btn-close">✕</button>
        </div>
        <form @submit.prevent="saveCliente" class="modal-form">
          <div class="form-group">
            <label>Nombre Completo*</label>
            <input v-model="formData.nombre" type="text" required />
          </div>
          <div class="form-group">
            <label>Número de Contacto</label>
            <input v-model="formData.telefono" type="tel" />
          </div>
          <div class="form-group">
            <label>Ubicación (Dirección)</label>
            <input v-model="formData.direccion" type="text" />
          </div>
          <div class="form-group">
            <label>Preferencias y Notas</label>
            <textarea v-model="formData.notas" rows="3"></textarea>
          </div>
          <div class="modal-actions">
            <button type="submit" class="btn btn-primary">Guardar Cliente</button>
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
const currentPage = ref(1)

const formData = ref({
  nombre: '',
  telefono: '',
  direccion: '',
  notas: ''
})

const clientes = ref([
  { id: 1, nombre: 'María González', telefono: '555-5551', direccion: 'Zona 1', ultimaCompra: '15-11-2024', compras: '23', totaciones: '8', acciones: '8' },
  { id: 2, nombre: 'Chimaltenango', telefono: '555-5555', direccion: 'Chimaltenango', ultimaCompra: '10-11-2024', compras: '15', totaciones: '8', acciones: '8' },
  { id: 3, nombre: 'Ana Pérez', telefono: '555-5567', direccion: 'Antigua', ultimaCompra: '08-11-2024', compras: '12', totaciones: '8', acciones: '8' },
  { id: 4, nombre: 'Carmen Hern..', telefono: '555-5567', direccion: 'Guatemala', ultimaCompra: '05-11-2024', compras: '8', totaciones: '8', acciones: '8' },
  { id: 5, nombre: 'Carmen hendez', telefono: '555-5855', direccion: 'Mixco', ultimaCompra: '01-11-2024', compras: '5', totaciones: '0-20 3 €', acciones: '8' }
])

const filteredClientes = computed(() => {
  if (!searchQuery.value) return clientes.value
  return clientes.value.filter(c => c.nombre.toLowerCase().includes(searchQuery.value.toLowerCase()) || c.telefono.includes(searchQuery.value))
})

const openModal = (cliente = null) => {
  if (cliente) {
    formData.value = { ...cliente }
  } else {
    formData.value = { nombre: '', telefono: '', direccion: '', notas: '' }
  }
  showModal.value = true
}

const closeModal = () => {
  showModal.value = false
}

const saveCliente = () => {
  clientes.value.push({ ...formData.value, id: clientes.value.length + 1, ultimaCompra: 'N/A', compras: '0', totaciones: '0', acciones: '0' })
  toast.success('Cliente guardado')
  closeModal()
}

const deleteCliente = (id) => {
  if (confirm('¿Eliminar cliente?')) {
    clientes.value = clientes.value.filter(c => c.id !== id)
    toast.success('Cliente eliminado')
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

.btn-icon,
.btn-action {
  background: transparent;
  border: none;
  cursor: pointer;
  padding: 4px 8px;
  margin: 0 4px;
  font-size: 14px;
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

.form-group input,
.form-group textarea {
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
</style>