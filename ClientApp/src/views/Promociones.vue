<template>
  <div class="page-container">
    <div class="page-header">
      <h1>Gestión de Promociones</h1>
      <button @click="openModal()" class="btn btn-primary">+ Nueva Promoción</button>
    </div>

    <div class="filters-bar">
      <div class="search-input-wrapper">
        <span class="search-icon">🔍</span>
        <input 
          v-model="searchQuery" 
          type="text" 
          placeholder="Buscar promoción..." 
          class="search-input"
          @input="filtrarPromociones"
        />
      </div>
      <select v-model="filterEstado" class="filter-select" @change="filtrarPromociones">
        <option value="all">Estado: Todas</option>
        <option value="activa">Activas</option>
        <option value="programada">Programadas</option>
        <option value="finalizada">Finalizadas</option>
      </select>
    </div>

    <!-- Estado de carga -->
    <div v-if="loading" class="loading-state">
      <div class="spinner"></div>
      <p>Cargando promociones...</p>
    </div>

    <!-- Promociones Activas -->
    <div v-else class="promotions-section">
      <h2 class="section-title">Promociones Activas</h2>
      <div v-if="activePromociones.length === 0" class="empty-state">
        <p>No hay promociones activas en este momento</p>
      </div>
      <div v-else class="promotions-grid">
        <div v-for="promo in activePromociones" :key="promo.id" class="promo-card active">
          <div class="promo-badge">🔥 ACTIVA</div>
          <h3>{{ promo.nombre }}</h3>
          <p class="promo-description">{{ promo.descripcion }}</p>
          <div class="promo-details">
            <div class="promo-detail">
              <span class="label">Producto:</span>
              <span class="value">{{ promo.productoNombre || promo.producto }}</span>
            </div>
            <div class="promo-detail">
              <span class="label">Descuento:</span>
              <span class="value discount">{{ formatearDescuento(promo) }}</span>
            </div>
            <div class="promo-detail">
              <span class="label">Válido hasta:</span>
              <span class="value">{{ formatearFecha(promo.fechaFin) }}</span>
            </div>
            <div class="promo-detail progress">
              <span class="label">Progreso:</span>
              <div class="progress-bar">
                <div class="progress-fill" :style="{ width: calcularProgreso(promo) + '%' }"></div>
              </div>
            </div>
          </div>
          <div class="promo-actions">
            <button @click="openModal(promo)" class="btn btn-secondary btn-sm">Editar</button>
            <button @click="finalizarPromocion(promo.id)" class="btn btn-warning btn-sm">Finalizar</button>
          </div>
        </div>
      </div>
    </div>

    <!-- Tabla de todas las promociones -->
    <div v-if="!loading" class="all-promotions">
      <h2 class="section-title">Historial de Promociones</h2>
      <div class="table-container">
        <table class="data-table">
          <thead>
            <tr>
              <th>NOMBRE</th>
              <th>PRODUCTO</th>
              <th>TIPO</th>
              <th>DESCUENTO</th>
              <th>FECHA INICIO</th>
              <th>FECHA FIN</th>
              <th>ESTADO</th>
              <th>ACCIONES</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="promo in paginatedPromociones" :key="promo.id">
              <td><strong>{{ promo.nombre }}</strong></td>
              <td>{{ promo.productoNombre || promo.producto }}</td>
              <td><span class="badge-tipo">{{ promo.tipo }}</span></td>
              <td><span class="discount-badge">{{ formatearDescuento(promo) }}</span></td>
              <td>{{ formatearFecha(promo.fechaInicio) }}</td>
              <td>{{ formatearFecha(promo.fechaFin) }}</td>
              <td><span :class="['status', `status-${promo.estado}`]">{{ promo.estado.toUpperCase() }}</span></td>
              <td>
                <button @click="openModal(promo)" class="btn-icon" title="Editar">✏️</button>
                <button @click="deletePromocion(promo.id)" class="btn-icon" title="Eliminar">🗑️</button>
              </td>
            </tr>
          </tbody>
        </table>
        <div v-if="filteredPromociones.length === 0" class="no-data">
          <p>No se encontraron promociones</p>
        </div>
      </div>
    </div>

    <!-- Paginación -->
    <div v-if="totalPages > 1 && !loading" class="pagination-container">
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
          <h2>{{ isEditing ? 'Editar Promoción' : 'Nueva Promoción' }}</h2>
          <button @click="closeModal" class="btn-close">✕</button>
        </div>
        <form @submit.prevent="savePromocion" class="modal-form">
          <div class="form-group">
            <label>Nombre de la Promoción*</label>
            <input v-model="formData.nombre" type="text" required placeholder="Ej: Descuento Black Friday" />
          </div>
          <div class="form-group">
            <label>Descripción</label>
            <textarea v-model="formData.descripcion" rows="3" placeholder="Describe la promoción..."></textarea>
          </div>
          <div class="form-row">
            <div class="form-group">
              <label>Producto*</label>
              <select v-model="formData.productoId" required class="form-select">
                <option value="">Seleccionar producto...</option>
                <option value="todos">Todos los Productos</option>
                <option value="corte">Corte Quiché</option>
                <option value="huipil">Huipil Nebaj</option>
                <option value="blusa">Blusa Bordada</option>
                <option value="perraje">Perraje</option>
              </select>
            </div>
            <div class="form-group">
              <label>Tipo de Descuento*</label>
              <select v-model="formData.tipo" required>
                <option value="Porcentaje">Porcentaje (%)</option>
                <option value="Monto">Monto Fijo (Q)</option>
              </select>
            </div>
          </div>
          <div class="form-row">
            <div class="form-group">
              <label>Valor del Descuento*</label>
              <input v-model.number="formData.valorDescuento" type="number" step="0.01" min="0" required />
            </div>
            <div class="form-group">
              <label>{{ formData.tipo === 'Porcentaje' ? '% de descuento' : 'Monto en Q' }}</label>
            </div>
          </div>
          <div class="form-row">
            <div class="form-group">
              <label>Fecha Inicio*</label>
              <input v-model="formData.fechaInicio" type="date" required />
            </div>
            <div class="form-group">
              <label>Fecha Fin*</label>
              <input v-model="formData.fechaFin" type="date" required />
            </div>
          </div>

          <div v-if="formError" class="error-message">
            {{ formError }}
          </div>

          <div class="modal-actions">
            <button type="submit" class="btn btn-primary" :disabled="loadingForm">
              {{ loadingForm ? 'Guardando...' : (isEditing ? 'Actualizar Promoción' : 'Crear Promoción') }}
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
const promociones = ref([])
const filteredPromociones = ref([])
const loading = ref(false)
const loadingForm = ref(false)
const showModal = ref(false)
const isEditing = ref(false)
const searchQuery = ref('')
const filterEstado = ref('all')
const currentPage = ref(1)
const itemsPerPage = 10
const formError = ref('')

// Formulario
const formData = ref({
  id: null,
  nombre: '',
  descripcion: '',
  productoId: '',
  producto: '',
  tipo: 'Porcentaje',
  valorDescuento: '',
  fechaInicio: '',
  fechaFin: '',
  estado: 'activa'
})

// Computed
const totalPages = computed(() => Math.ceil(filteredPromociones.value.length / itemsPerPage))

const paginatedPromociones = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage
  const end = start + itemsPerPage
  return filteredPromociones.value.slice(start, end)
})

const activePromociones = computed(() => 
  filteredPromociones.value.filter(p => p.estado === 'activa')
)

// Métodos
const cargarPromociones = async () => {
  loading.value = true
  try {
    const response = await api.get('/Promociones')
    promociones.value = response.data.data || response.data || []
    actualizarEstados()
    filtrarPromociones()
  } catch (error) {
    console.error('Error al cargar promociones:', error)
    // Usar datos de ejemplo si la API no responde
    promociones.value = [
      { 
        id: 1, 
        nombre: 'Liquidación Cortes', 
        descripcion: 'Descuento en cortes de temporada', 
        producto: 'Corte Quiché',
        productoNombre: 'Corte Quiché',
        tipo: 'Porcentaje', 
        valorDescuento: 15,
        descuento: '15%', 
        fechaInicio: '2025-11-01', 
        fechaFin: '2025-11-30', 
        estado: 'activa' 
      },
      { 
        id: 2, 
        nombre: 'Promo Huipiles', 
        descripcion: '2x1 en huipiles seleccionados', 
        producto: 'Huipil Nebaj',
        productoNombre: 'Huipil Nebaj',
        tipo: 'Monto', 
        valorDescuento: 200,
        descuento: 'Q 200', 
        fechaInicio: '2025-10-15', 
        fechaFin: '2025-11-15', 
        estado: 'activa' 
      },
      { 
        id: 3, 
        nombre: 'Black Friday', 
        descripcion: 'Descuento especial fin de año', 
        producto: 'Todos',
        productoNombre: 'Todos los Productos',
        tipo: 'Porcentaje', 
        valorDescuento: 25,
        descuento: '25%', 
        fechaInicio: '2025-11-25', 
        fechaFin: '2025-11-30', 
        estado: 'programada' 
      },
      { 
        id: 4, 
        nombre: 'Verano 2024', 
        descripcion: 'Liquidación verano', 
        producto: 'Blusas',
        productoNombre: 'Blusa Bordada',
        tipo: 'Porcentaje', 
        valorDescuento: 20,
        descuento: '20%', 
        fechaInicio: '2024-06-01', 
        fechaFin: '2024-08-31', 
        estado: 'finalizada' 
      }
    ]
    actualizarEstados()
    filtrarPromociones()
    toast.warning('Usando datos de ejemplo')
  } finally {
    loading.value = false
  }
}

const actualizarEstados = () => {
  const ahora = new Date()
  promociones.value.forEach(promo => {
    const inicio = new Date(promo.fechaInicio)
    const fin = new Date(promo.fechaFin)
    
    if (promo.estado !== 'finalizada') {
      if (ahora < inicio) {
        promo.estado = 'programada'
      } else if (ahora > fin) {
        promo.estado = 'finalizada'
      } else {
        promo.estado = 'activa'
      }
    }
  })
}

const filtrarPromociones = () => {
  let filtered = promociones.value

  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(p =>
      p.nombre.toLowerCase().includes(query) ||
      (p.productoNombre || p.producto).toLowerCase().includes(query)
    )
  }

  if (filterEstado.value !== 'all') {
    filtered = filtered.filter(p => p.estado === filterEstado.value)
  }

  filteredPromociones.value = filtered
  currentPage.value = 1
}

const formatearFecha = (fecha) => {
  if (!fecha) return 'N/A'
  const date = new Date(fecha)
  return date.toLocaleDateString('es-GT')
}

const formatearDescuento = (promo) => {
  if (promo.tipo === 'Porcentaje') {
    return `${promo.valorDescuento || 0}%`
  }
  return `Q ${promo.valorDescuento || 0}`
}

const calcularProgreso = (promo) => {
  const inicio = new Date(promo.fechaInicio).getTime()
  const fin = new Date(promo.fechaFin).getTime()
  const ahora = new Date().getTime()
  
  if (ahora < inicio) return 0
  if (ahora > fin) return 100
  
  const duracion = fin - inicio
  const transcurrido = ahora - inicio
  return Math.round((transcurrido / duracion) * 100)
}

const openModal = (promo = null) => {
  formError.value = ''
  if (promo) {
    isEditing.value = true
    formData.value = { 
      ...promo,
      productoId: promo.productoId || '',
      valorDescuento: promo.valorDescuento || 0
    }
  } else {
    isEditing.value = false
    formData.value = { 
      id: null,
      nombre: '', 
      descripcion: '', 
      productoId: '', 
      producto: '',
      tipo: 'Porcentaje', 
      valorDescuento: 0,
      fechaInicio: '',
      fechaFin: '',
      estado: 'activa'
    }
  }
  showModal.value = true
}

const closeModal = () => {
  showModal.value = false
  formData.value = { 
    id: null,
    nombre: '', 
    descripcion: '', 
    productoId: '', 
    producto: '',
    tipo: 'Porcentaje', 
    valorDescuento: 0,
    fechaInicio: '',
    fechaFin: '',
    estado: 'activa'
  }
}

const savePromocion = async () => {
  formError.value = ''
  
  // Validaciones
  if (!formData.value.nombre || !formData.value.productoId || formData.value.valorDescuento <= 0 || 
      !formData.value.fechaInicio || !formData.value.fechaFin) {
    formError.value = 'Por favor completa todos los campos requeridos'
    return
  }

  if (new Date(formData.value.fechaInicio) >= new Date(formData.value.fechaFin)) {
    formError.value = 'La fecha de inicio debe ser menor a la fecha de fin'
    return
  }

  if (formData.value.tipo === 'Porcentaje' && (formData.value.valorDescuento < 0 || formData.value.valorDescuento > 100)) {
    formError.value = 'El porcentaje debe estar entre 0 y 100'
    return
  }

  loadingForm.value = true

  try {
    // Mapear IDs de productos a nombres
    const productosMap = {
      'todos': 'Todos los Productos',
      'corte': 'Corte Quiché',
      'huipil': 'Huipil Nebaj',
      'blusa': 'Blusa Bordada',
      'perraje': 'Perraje'
    }

    const promoData = {
      nombre: formData.value.nombre,
      descripcion: formData.value.descripcion,
      productoNombre: productosMap[formData.value.productoId] || formData.value.productoId,
      tipo: formData.value.tipo,
      valorDescuento: formData.value.valorDescuento,
      fechaInicio: formData.value.fechaInicio,
      fechaFin: formData.value.fechaFin,
      estado: formData.value.estado || 'activa'
    }

    if (isEditing.value) {
      const response = await api.put(`/Promociones/${formData.value.id}`, promoData)
      if (response.data.success) {
        toast.success('Promoción actualizada exitosamente')
      }
    } else {
      const response = await api.post('/Promociones', promoData)
      if (response.data.success) {
        toast.success('Promoción creada exitosamente')
      }
    }

    cargarPromociones()
    closeModal()
  } catch (error) {
    console.error('Error al guardar promoción:', error)
    formError.value = error.response?.data?.message || 'Error al guardar la promoción'
    toast.error(formError.value)
  } finally {
    loadingForm.value = false
  }
}

const finalizarPromocion = async (id) => {
  if (!confirm('¿Finalizar esta promoción?')) return

  try {
    const promo = promociones.value.find(p => p.id === id)
    if (promo) {
      const response = await api.put(`/Promociones/${id}`, {
        ...promo,
        estado: 'finalizada'
      })
      
      if (response.data.success) {
        toast.success('Promoción finalizada exitosamente')
        cargarPromociones()
      }
    }
  } catch (error) {
    console.error('Error al finalizar promoción:', error)
    toast.error('Error al finalizar la promoción')
  }
}

const deletePromocion = async (id) => {
  if (!confirm('¿Eliminar esta promoción?')) return

  try {
    const response = await api.delete(`/Promociones/${id}`)
    if (response.data.success) {
      toast.success('Promoción eliminada exitosamente')
      cargarPromociones()
    }
  } catch (error) {
    console.error('Error al eliminar promoción:', error)
    toast.error('Error al eliminar la promoción')
  }
}

const previousPage = () => {
  if (currentPage.value > 1) currentPage.value--
}

const nextPage = () => {
  if (currentPage.value < totalPages.value) currentPage.value++
}

onMounted(() => {
  cargarPromociones()
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
  margin-bottom: 30px;
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
  min-width: 200px;
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

.promotions-section {
  margin-bottom: 40px;
}

.section-title {
  font-family: var(--font-title);
  font-size: 22px;
  color: var(--color-text-primary);
  margin: 0 0 20px 0;
}

.empty-state {
  text-align: center;
  padding: 40px 20px;
  color: var(--color-text-muted);
}

.promotions-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
  gap: 20px;
}

.promo-card {
  background: white;
  border: 2px solid var(--color-border);
  border-radius: var(--border-radius-lg);
  padding: 24px;
  position: relative;
}

.promo-card.active {
  border-color: var(--color-success);
  background: linear-gradient(135deg, #ffffff 0%, #f0fff4 100%);
}

.promo-badge {
  position: absolute;
  top: -12px;
  right: 20px;
  background: var(--color-success);
  color: white;
  padding: 6px 16px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: var(--font-weight-bold);
}

.promo-card h3 {
  font-family: var(--font-title);
  font-size: 20px;
  color: var(--color-primary);
  margin: 0 0 8px 0;
}

.promo-description {
  font-size: 14px;
  color: var(--color-text-secondary);
  margin: 0 0 16px 0;
}

.promo-details {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: 16px;
}

.promo-detail {
  display: flex;
  justify-content: space-between;
  font-size: 14px;
}

.promo-detail.progress {
  flex-direction: column;
  gap: 8px;
}

.promo-detail .label {
  color: var(--color-text-secondary);
}

.promo-detail .value {
  font-weight: var(--font-weight-medium);
  color: var(--color-text-primary);
}

.promo-detail .discount {
  color: var(--color-success);
  font-weight: var(--font-weight-bold);
  font-size: 16px;
}

.progress-bar {
  width: 100%;
  height: 6px;
  background: var(--color-background-secondary);
  border-radius: 3px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  background: linear-gradient(90deg, var(--color-success), var(--color-accent));
  transition: width 0.3s ease;
}

.promo-actions {
  display: flex;
  gap: 8px;
}

.all-promotions {
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

.badge-tipo {
  display: inline-block;
  padding: 4px 12px;
  background: var(--color-info-light);
  color: var(--color-info);
  border-radius: 12px;
  font-size: 12px;
  font-weight: var(--font-weight-bold);
}

.discount-badge {
  display: inline-block;
  background: var(--color-success-light);
  color: var(--color-success);
  padding: 4px 12px;
  border-radius: 12px;
  font-weight: var(--font-weight-bold);
}

.status {
  display: inline-block;
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: var(--font-weight-bold);
}

.status-activa {
  background: var(--color-success-light);
  color: var(--color-success);
}

.status-programada {
  background: var(--color-info-light);
  color: var(--color-info);
}

.status-finalizada {
  background: var(--color-background-secondary);
  color: var(--color-text-muted);
}

.btn-icon {
  background: transparent;
  border: none;
  cursor: pointer;
  padding: 4px 8px;
  font-size: 18px;
  margin: 0 4px;
}

.no-data {
  padding: 40px;
  text-align: center;
  color: var(--color-text-muted);
}

.pagination-container {
  display: flex;
  justify-content: center;
  margin-top: 30px;
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
  padding: 20px;
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
  color: var(--color-text-primary);
}

.form-group input,
.form-group select,
.form-group textarea {
  width: 100%;
  padding: 12px 16px;
  border: 2px solid var(--color-border);
  border-radius: var(--border-radius-md);
  font-size: 15px;
  font-family: var(--font-body);
}

.form-group input:focus,
.form-group select:focus,
.form-group textarea:focus {
  outline: none;
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px rgba(27, 75, 127, 0.1);
}

.form-select {
  width: 100%;
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

.btn-sm {
  padding: 8px 16px;
  font-size: 13px;
}

@media (max-width: 768px) {
  .page-container {
    padding: 20px;
  }

  .form-row {
    grid-template-columns: 1fr;
  }

  .promotions-grid {
    grid-template-columns: 1fr;
  }

  .page-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 16px;
  }

  .filters-bar {
    flex-direction: column;
  }

  .search-input-wrapper,
  .filter-select {
    width: 100%;
  }
}
</style>