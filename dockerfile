# --- Build Stage - Frontend (Node.js/Vite/Vue) ---
FROM node:18-alpine AS frontend-build
WORKDIR /app/ClientApp
# Copia solo los archivos de dependencias para aprovechar el cache
COPY ClientApp/package*.json ./
# Instala las dependencias
RUN npm ci
# Copia el código fuente del frontend
COPY ClientApp/ ./
# Compila el frontend
RUN npm run build

# --- Build Stage - Backend (.NET SDK) ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /app
# Copia los archivos de proyecto para aprovechar el cache de dotnet restore
COPY *.csproj ./
RUN dotnet restore
# Copia el resto del código fuente del backend
COPY . .
# Publica la aplicación. Se añade '/p:UseAppHost=false' para asegurar compatibilidad
# con el entorno flexible de App Engine en Linux.
RUN dotnet publish -c Release -o out /p:UseAppHost=false

# --- Runtime Stage (.NET ASPNET) ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
# Copia el backend publicado
COPY --from=backend-build /app/out .
# Copia el frontend compilado a la carpeta wwwroot del backend.
# NOTA: La ruta de origen (/app/wwwroot) es la que indicaste originalmente.
# Si tu compilación de Node.js produce la salida en /app/ClientApp/dist, cambia la línea a:
# COPY --from=frontend-build /app/ClientApp/dist ./wwwroot
COPY --from=frontend-build /app/wwwroot ./wwwroot

# Configuración de App Engine
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Punto de entrada de la aplicación
ENTRYPOINT ["dotnet", "InventarioRopaTipica.dll"]