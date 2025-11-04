<template>
  <div class="login-container">
    <div class="login-card">

      <!-- Título -->
      <h1 class="title">COMERCIALES EMILIAS</h1>
      <p class="subtitle">Sistema de Gestión Inventario</p>

      <!-- Formulario -->
      <form @submit.prevent="handleLogin" class="login-form">
        <!-- Correo Electrónico -->
        <div class="form-group">
          <label for="email">Correo Electrónico</label>
          <div class="input-wrapper">
            <div class="input-icon">
              <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor">
                <path d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z"/>
              </svg>
            </div>
            <input
              id="email"
              v-model="credentials.email"
              type="email"
              placeholder="usuario@ejemplo.com"
              required
              :disabled="loading"
            />
          </div>
        </div>

        <!-- Contraseña -->
        <div class="form-group">
          <label for="password">Contraseña</label>
          <div class="input-wrapper">
            <div class="input-icon">
              <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor">
                <path d="M18 8h-1V6c0-2.76-2.24-5-5-5S7 3.24 7 6v2H6c-1.1 0-2 .9-2 2v10c0 1.1.9 2 2 2h12c1.1 0 2-.9 2-2V10c0-1.1-.9-2-2-2zm-6 9c-1.1 0-2-.9-2-2s.9-2 2-2 2 .9 2 2-.9 2-2 2zm3.1-9H8.9V6c0-1.71 1.39-3.1 3.1-3.1 1.71 0 3.1 1.39 3.1 3.1v2z"/>
              </svg>
            </div>
            <input
              id="password"
              v-model="credentials.password"
              type="password"
              placeholder="••••••••"
              required
              :disabled="loading"
            />
          </div>
        </div>

        <!-- Mensaje de error -->
        <div v-if="errorMessage" class="error-message">
          {{ errorMessage }}
        </div>

        <!-- Botón de login -->
        <button type="submit" class="btn-login" :disabled="loading">
          <span v-if="!loading">INGRESAR AL SISTEMA</span>
          <span v-else>INGRESANDO...</span>
        </button>

        <!-- Olvidaste tu contraseña -->
        <a href="#" class="forgot-password">¿Olvidaste tu contraseña?</a>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { useToast } from 'vue-toastification'

const router = useRouter()
const authStore = useAuthStore()
const toast = useToast()

const credentials = ref({
  email: '',
  password: ''
})

const loading = ref(false)
const errorMessage = ref('')

const handleLogin = async () => {
  loading.value = true
  errorMessage.value = ''

  try {
    const result = await authStore.login(credentials.value)
    
    if (result.success) {
      toast.success('¡Bienvenido!')
      router.push('/')
    } else {
      errorMessage.value = result.message
      toast.error(result.message)
    }
  } catch (error) {
    errorMessage.value = 'Error al conectar con el servidor'
    toast.error('Error al conectar con el servidor')
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.login-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background: #F5E6D3;
  padding: 20px;
  font-family: var(--font-body);
}

.login-card {
  background: white;
  border-radius: 20px;
  border: 3px solid #1a1a1a;
  padding: 50px 60px;
  width: 100%;
  max-width: 480px;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.15);
}

/* Logo */
.logo-box {
  width: 70px;
  height: 70px;
  background: var(--color-accent);
  margin: 0 auto 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
  font-size: 14px;
  color: white;
  border-radius: var(--border-radius-md);
}

/* Títulos */
.title {
  color: var(--color-primary);
  font-size: 32px;
  font-weight: bold;
  margin: 0 0 8px 0;
  text-align: center;
  letter-spacing: 0.5px;
  font-family: var(--font-title);
}

.subtitle {
  color: var(--color-text-secondary);
  font-size: 16px;
  margin: 0 0 40px 0;
  text-align: center;
  font-weight: 400;
  font-family: var(--font-body);
}

/* Formulario */
.login-form {
  display: flex;
  flex-direction: column;
}

.form-group {
  margin-bottom: 24px;
}

.form-group label {
  display: block;
  color: var(--color-primary);
  font-weight: 600;
  font-size: 15px;
  margin-bottom: 8px;
  font-family: var(--font-body);
}

/* Input wrapper con icono */
.input-wrapper {
  position: relative;
  display: flex;
  align-items: stretch;
  height: 50px;
}

.input-icon {
  width: 50px;
  min-width: 50px;
  background: var(--color-accent);
  border: 2px solid #1a1a1a;
  border-right: none;
  border-radius: 8px 0 0 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.input-icon svg {
  width: 22px;
  height: 22px;
  color: white;
  flex-shrink: 0;
}

.input-wrapper input {
  flex: 1;
  padding: 0 16px;
  border: 2px solid #1a1a1a;
  border-left: none;
  border-radius: 0 8px 8px 0;
  font-size: 15px;
  transition: all 0.3s;
  background: white;
  font-family: var(--font-body);
  height: 100%;
}

.input-wrapper input::placeholder {
  color: #9CA3AF;
}

.input-wrapper input:focus {
  outline: none;
  border-color: var(--color-primary);
}

.input-wrapper input:focus ~ .input-icon {
  border-color: var(--color-primary);
}

.input-wrapper input:disabled {
  background-color: #f5f5f5;
  cursor: not-allowed;
}

/* Mensaje de error */
.error-message {
  background-color: var(--color-error-light);
  color: #991b1b;
  padding: 12px 16px;
  border-radius: var(--border-radius-md);
  font-size: 14px;
  text-align: center;
  margin-bottom: 20px;
  border: 1px solid var(--color-error);
  font-family: var(--font-body);
}

/* Botón de login */
.btn-login {
  background: var(--color-primary);
  color: white;
  padding: 16px;
  border: 2px solid #1a1a1a;
  border-radius: var(--border-radius-md);
  font-size: 16px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.3s;
  letter-spacing: 0.5px;
  text-transform: uppercase;
  margin-top: 10px;
  font-family: var(--font-body);
}

.btn-login:hover:not(:disabled) {
  background: var(--color-primary-dark);
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(27, 75, 127, 0.3);
}

.btn-login:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
}

/* Enlace olvidaste contraseña */
.forgot-password {
  text-align: center;
  display: block;
  margin-top: 20px;
  color: var(--color-primary);
  font-size: 14px;
  text-decoration: underline;
  cursor: pointer;
  transition: color 0.3s;
  font-family: var(--font-body);
}

.forgot-password:hover {
  color: var(--color-primary-dark);
}

/* Responsive */
@media (max-width: 600px) {
  .login-card {
    padding: 40px 30px;
  }

  .title {
    font-size: 26px;
  }

  .subtitle {
    font-size: 14px;
  }

  .input-wrapper {
    height: 45px;
  }

  .input-icon {
    width: 45px;
    min-width: 45px;
  }

  .input-icon svg {
    width: 20px;
    height: 20px;
  }
}
</style>