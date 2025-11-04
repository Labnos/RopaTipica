import api from './api'

export default {
  async getResumen() {
    const { data } = await api.get('/Ventas/Resumen')
    return data
  },
  async getTopProductos(topN = 5) {
    const { data } = await api.get('/Ventas/TopProductos', { params: { topN } })
    return data
  },
  async getVentasMensuales(year) {
    const { data } = await api.get('/Ventas/VentasMensuales', { params: { year } })
    return data
  },
  async getBajoStock() {
    const { data } = await api.get('/Productos/bajo-stock')
    return data
  }
}
