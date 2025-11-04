# Build Stage - Frontend
FROM node:18-alpine AS frontend-build
WORKDIR /app/ClientApp
COPY ClientApp/package*.json ./
RUN npm ci
COPY ClientApp/ ./
RUN npm run build

# Build Stage - Backend
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /app
COPY *.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet publish -c Release -o out

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=backend-build /app/out .
COPY --from=frontend-build /app/wwwroot ./wwwroot

# Exponer puerto
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "InventarioRopaTipica.dll"]
```

### **.dockerignore:**
```
**/node_modules
**/bin
**/obj
**/.git
**/.vs
**/.vscode
**/wwwroot/dist
ClientApp/dist