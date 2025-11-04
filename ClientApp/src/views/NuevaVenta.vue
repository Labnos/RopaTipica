<template>
  <div class="page-container">
    <div class="page-header">
      <h1>Nueva Venta</h1>
      <button @click="$router.push('/ventas')" class="btn btn-secondary">← Volver a Ventas</button>
    </div>

    <div class="sale-form">
      <!-- Información del Cliente -->
      <div class="form-section">
        <h2 class="section-title">Información del Cliente</h2>
        <div class="form-row">
          <div class="form-group">
            <label>Cliente*</label>
            <select v-model="venta.clienteId" class="form-select">
              <option value="">Seleccionar cliente...</option>
              <option value="general">Cliente General</option>
              <option v-for="cliente in clientes" :key="cliente.id" :value="cliente.id">
                {{ cliente.nombre }}
              </option>
            </select>
          </div>
          <div class="form-group">
            <label>Canal de Venta*</label>
            <select v-model="venta.canal" class="form-select">
              <option value="Fisico">Tienda Física</option>
              <option value="Facebook">Facebook</option>
              <option value="WhatsApp">WhatsApp</option>
            </select>
          </div>
        </div>
      </div>

      <!-- Agregar Productos -->
      <div class="form-section">
        <div class="section-header">
          <h2 class="section-title">Productos</h2>
          <button @click="showProductModal = true" class="btn btn-primary">+ Agregar Producto</button>
        </div>

        <div v-if="venta.productos.length === 0" class="empty-state">
          <p>🛒 No hay productos agregados</p>
          <p class="empty-subtitle">Haz clic en "Agregar Producto" para comenzar</p>
        </div>

        <div v-else class="products-table">
          <table class="data-table">
            <thead>
              <tr>
                <th>PRODUCTO</th>
                <th>TIPO</th>
                <th>CANTIDAD/VARAS</th>
                <th>PRECIO UNITARIO</th>
                <th>TIPO VENTA</th>
                <th>SUBTOTAL</th>
                <th>ACCIONES</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(item, index) in venta.productos" :key="index">
                <td><strong>{{ item.nombre }}</strong></td>
                <td>{{ item.tipo }}</td>
                <td>
                  <input 
                    v-model.number="item.cantidad" 
                    type="number" 
                    min="0.1" 
                    step="0.1"
                    @change="calcularSubtotal(item)"
                    class="input-qty"
                  />
                  {{ item.tipo === 'Corte' ? 'varas' : 'unidades' }}
                </td>
                <td>Q {{ item.precioUnitario.toFixed(2) }}</td>
                <td>
                  <span :class="['badge-tipo', item.tipoVenta === 'Parcial' ? 'badge-parcial' : 'badge-completo']">
                    {{ item.tipoVenta }}
                  </span>
                  <span v-if="item.tipoVenta === 'Parcial'" class="warning-text">⚠️ No permite devolución</span>
                </td>
                <td><strong>Q {{ item.subtotal.toFixed(2) }}</strong></td>
                <td>
                  <button @click="removeProducto(index)" class="btn-icon" title="Eliminar">🗑️</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Resumen de Venta -->
      <div class="form-section summary-section">
        <h2 class="section-title">Resumen de Venta</h2>
        <div class="summary-grid">
          <div class="summary-item">
            <span class="label">Subtotal:</span>
            <span class="value">Q {{ subtotal.toFixed(2) }}</span>
          </div>
          <div class="summary-item">
            <span class="label">Descuento:</span>
            <input v-model.number="venta.descuento" type="number" min="0" step="0.01" class="input-discount" />
          </div>
          <div class="summary-item">
            <span class="label">IVA (12%):</span>
            <span class="value">Q {{ iva.toFixed(2) }}</span>
          </div>
          <div class="summary-item total">
            <span class="label">TOTAL:</span>
            <span class="value">Q {{ total.toFixed(2) }}</span>
          </div>
        </div>
      </div>

      <!-- Información de Pago -->
      <div class="form-section">
        <h2 class="section-title">Información de Pago</h2>
        <div class="form-row">
          <div class="form-group">
            <label>Método de Pago*</label>
            <select v-model="venta.metodoPago" class="form-select">
              <option value="Efectivo">Efectivo</option>
              <option value="Tarjeta">Tarjeta</option>
              <option value="Transferencia">Transferencia</option>
              <option value="Cheque">Cheque</option>
            </select>
          </div>
          <div class="form-group">
            <label>Estado de Pago*</label>
            <select v-model="venta.estadoPago" class="form-select">
              <option value="Pagado">Pagado</option>
              <option value="Pendiente">Pendiente</option>
            </select>
          </div>
        </div>
        <div class="form-group">
          <label>Observaciones</label>
          <textarea v-model="venta.observaciones" rows="3" class="form-textarea" placeholder="Notas adicionales sobre la venta..."></textarea>
        </div>
      </div>

      <!-- Acciones -->
      <div class="form-actions">
        <button @click="guardarVenta" class="btn btn-primary btn-lg" :disabled="!canSave">
          💾 Guardar Venta
        </button>
        <button @click="cancelar" class="btn btn-secondary btn-lg">
          Cancelar
        </button>
      </div>
    </div>

    <!-- Modal de Selección de Productos -->
    <div v-if="showProductModal" class="modal-overlay" @click.self="showProductModal = false">
      <div class="modal-card">
        <div class="modal-header">
          <h2>Seleccionar Producto</h2>
          <button @click="showProductModal = false" class="btn-close">✕</button>
        </div>
        <div class="modal-body">
          <div class="search-bar">
            <input v-model="searchProducto" type="text" placeholder="Buscar producto..." class="search-input" />
          </div>
          <div class="products-list">
            <div 
              v-for="producto in filteredProductos" 
              :key="producto.id" 
              @click="selectProducto(producto)"
              class="product-item"
            >
              <div class="product-info">
                <strong>{{ producto.nombre }}</strong>
                <span class="product-type">{{ producto.tipo }}</span>
              </div>
              <div class="product-price">
                <span>Q {{ producto.precioVenta.toFixed(2) }}</span>
                <span class="product-stock">Stock: {{ producto.stock }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'vue-toastification'

const router = useRouter()
const toast = useToast()

const showProductModal = ref(false)
const searchProducto = ref('')

const clientes = ref([
  { id: 1, nombre: 'María González' },
  { id: 2, nombre: 'Ana Pérez' },
  { id: 3, nombre: 'Carmen Hernández' }
])

const productosDisponibles = ref([
  { id: 1, nombre: 'Corte Quiché', tipo: 'Corte', precioVenta: 1000.00, stock: 15, varasDisponibles: 8 },
  { id: 2, nombre: 'Guipil Comalapa', tipo: 'Huipil', precioVenta: 1500.00, stock: 10, varasDisponibles: null },
  { id: 3, nombre: 'Blusa Quiché', tipo: 'Blusa', precioVenta: 350.00, stock: 25, varasDisponibles: null },
  { id: 4, nombre: 'Perraje Totonicapán', tipo: 'Perraje', precioVenta: 1750.00, stock: 5, varasDisponibles: null }
])

const venta = ref({
  clienteId: '',
  canal: 'Fisico',
  productos: [],
  descuento: 0,
  metodoPago: 'Efectivo',
  estadoPago: 'Pagado',
  observaciones: ''
})

const filteredProductos = computed(() => {
  if (!searchProducto.value) return productosDisponibles.value
  return productosDisponibles.value.filter(p => 
    p.nombre.toLowerCase().includes(searchProducto.value.toLowerCase())
  )
})

const subtotal = computed(() => {
  return venta.value.productos.reduce((sum, item) => sum + item.subtotal, 0)
})

const iva = computed(() => {
  return (subtotal.value - venta.value.descuento) * 0.12
})

const total = computed(() => {
  return subtotal.value - venta.value.descuento + iva.value
})

const canSave = computed(() => {
  return venta.value.clienteId && venta.value.productos.length > 0
})

const selectProducto = (producto) => {
  const cantidad = producto.tipo === 'Corte' ? 8 : 1
  const tipoVenta = producto.tipo === 'Corte' && cantidad >= 8 ? 'Completo' : (producto.tipo === 'Corte' ? 'Parcial' : 'Completo')
  
  const item = {
    productoId: producto.id,
    nombre: producto.nombre,
    tipo: producto.tipo,
    cantidad: cantidad,
    precioUnitario: producto.precioVenta,
    tipoVenta: tipoVenta,
    subtotal: cantidad * producto.precioVenta
  }
  
  venta.value.productos.push(item)
  showProductModal.value = false
  searchProducto.value = ''
  
  if (tipoVenta === 'Parcial') {
    toast.warning('Venta PARCIAL: No permite devolución')
  }
}

const calcularSubtotal = (item) => {
  item.subtotal = item.cantidad * item.precioUnitario
  
  // Validar tipo de venta para Cortes
  if (item.tipo === 'Corte') {
    if (item.cantidad >= 8) {
      item.tipoVenta = 'Completo'
    } else {
      item.tipoVenta = 'Parcial'
      toast.warning('Venta PARCIAL: No permite devolución')
    }
  }
}

const removeProducto = (index) => {
  venta.value.productos.splice(index, 1)
}

const guardarVenta = () => {
  if (!canSave.value) {
    toast.error('Complete los datos requeridos')
    return
  }
  
  // Aquí iría la llamada al API
  toast.success('Venta registrada exitosamente')
  router.push('/ventas')
}

const cancelar = () => {
  if (confirm('¿Cancelar la venta? Se perderán los datos no guardados')) {
    router.push('/ventas')
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

.sale-form {
  max-width: 1200px;
}

.form-section {
  background: white;
  border: 2px solid var(--color-border);
  border-radius: var(--border-radius-lg);
  padding: 24px;
  margin-bottom: 24px;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.section-title {
  font-family: var(--font-title);
  font-size: 20px;
  color: var(--color-primary);
  margin: 0 0 20px 0;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
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

.form-select,
.form-textarea {
  width: 100%;
  padding: 12px 16px;
  border: 2px solid var(--color-border);
  border-radius: var(--border-radius-md);
  font-size: 15px;
  font-family: var(--font-body);
}

.form-select:focus,
.form-textarea:focus {
  outline: none;
  border-color: var(--color-primary);
}

.empty-state {
  text-align: center;
  padding: 60px 20px;
  color: var(--color-text-muted);
}

.empty-state p {
  font-size: 18px;
  margin: 8px 0;
}

.empty-subtitle {
  font-size: 14px;
}

.products-table {
  overflow-x: auto;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
}

.data-table thead {
  background: var(--color-background-secondary);
}

.data-table th {
  padding: 12px;
  text-align: left;
  font-weight: var(--font-weight-semibold);
  font-size: 12px;
  text-transform: uppercase;
  border-bottom: 2px solid var(--color-border);
}

.data-table td {
  padding: 12px;
  font-size: 14px;
  border-bottom: 1px solid var(--color-background);
}

.input-qty {
  width: 80px;
  padding: 6px;
  border: 1px solid var(--color-border);
  border-radius: var(--border-radius-sm);
  text-align: center;
}

.badge-tipo {
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 11px;
  font-weight: var(--font-weight-bold);
  margin-right: 8px;
}

.badge-completo {
  background: var(--color-success-light);
  color: var(--color-success);
}

.badge-parcial {
  background: var(--color-warning-light);
  color: var(--color-warning);
}

.warning-text {
  font-size: 11px;
  color: var(--color-warning);
  display: block;
  margin-top: 4px;
}

.btn-icon {
  background: transparent;
  border: none;
  cursor: pointer;
  font-size: 18px;
  padding: 4px;
}

.summary-section {
  background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
  border: 2px solid var(--color-primary);
}

.summary-grid {
  display: grid;
  gap: 16px;
}

.summary-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 16px;
}

.summary-item .label {
  font-weight: var(--font-weight-medium);
  color: var(--color-text-secondary);
}

.summary-item .value {
  font-weight: var(--font-weight-bold);
  color: var(--color-text-primary);
}

.summary-item.total {
  padding-top: 16px;
  border-top: 2px solid var(--color-border);
  font-size: 24px;
}

.summary-item.total .value {
  color: var(--color-primary);
  font-family: var(--font-title);
}

.input-discount {
  width: 150px;
  padding: 8px;
  border: 2px solid var(--color-border);
  border-radius: var(--border-radius-md);
  text-align: right;
  font-weight: var(--font-weight-bold);
}

.form-actions {
  display: flex;
  gap: 16px;
  margin-top: 30px;
}

.btn-lg {
  padding: 16px 32px;
  font-size: 16px;
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
  width: 90%;
  max-width: 700px;
  max-height: 80vh;
  display: flex;
  flex-direction: column;
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

.modal-body {
  padding: 24px;
  overflow-y: auto;
}

.search-bar {
  margin-bottom: 20px;
}

.search-input {
  width: 100%;
  padding: 12px 16px;
  border: 2px solid var(--color-border);
  border-radius: var(--border-radius-md);
  font-size: 15px;
}

.products-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.product-item {
  padding: 16px;
  border: 2px solid var(--color-border);
  border-radius: var(--border-radius-md);
  cursor: pointer;
  transition: all 0.3s;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.product-item:hover {
  border-color: var(--color-primary);
  background: var(--color-background);
  transform: translateY(-2px);
}

.product-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.product-type {
  font-size: 12px;
  color: var(--color-text-secondary);
}

.product-price {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 4px;
}

.product-price span {
  font-weight: var(--font-weight-bold);
  color: var(--color-primary);
}

.product-stock {
  font-size: 12px;
  color: var(--color-text-muted);
  font-weight: normal !important;
}
</style>