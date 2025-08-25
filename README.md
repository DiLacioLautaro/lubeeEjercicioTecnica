# PruebaTecnica2025 — Paso a paso (sin Docker y con Docker)

Guía mínima para ejecutar **Backend (.NET 8)** y **Frontend (Vue 3 + Bootstrap)**.

---

## A) Sin Docker (modo dev local)

### 1) Requisitos

* **.NET 8 SDK**
* **Node.js 18/20+**
* **Base de datos**

  * Opción 1: **PostgreSQL 16 local**

    * Usuario: `postgres`
    * Contraseña: `postgres`
    * Crear DB: `PruebaTecnica2025Db`
  * Opción 2 (más simple): **SQLite** (no requiere instalación extra)

---

### 2) Configuración

**Backend** → `appsettings.Development.json`

**PostgreSQL**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=PruebaTecnica2025Db;Username=postgres;Password=postgres"
  },
  "DatabaseProvider": "PostgreSQL",
  "AllowedHosts": "*"
}
```

**SQLite**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=./Data/pt2025.db"
  },
  "DatabaseProvider": "SQLite",
  "AllowedHosts": "*"
}
```

**Frontend** → `FrontEnd/.env`

```env
# cuando el backend corre local (dotnet run / VS)
VITE_API_BASE=http://localhost:7257/api
```

---

### 3) Migraciones EF Core

Solo si vas a usar **Postgres o SQLite local**:

```bash
cd PruebaTecnica2025
dotnet tool restore
dotnet ef database update
```

Esto crea las tablas (`Publications`, `PublicationImages`).

---

### 4) Levantar Backend

**Visual Studio / Rider**
Abrir la solución y correr (`F5`).

**Terminal**

```bash
cd PruebaTecnica2025
dotnet restore
dotnet run
```

Por defecto expone: `https://localhost:7257`.

---

### 5) Levantar Frontend

```bash
cd FrontEnd
npm install
npm run dev
```

Frontend disponible en: [http://localhost:5173](http://localhost:5173)

---

## B) Con Docker (todo junto)

### 1) Requisitos

* **Windows/Mac**: Docker Desktop (esperar que diga *Engine running*).
* **Linux**: Docker Engine + plugin `docker compose`.

---

### 2) Archivos esperados en la raíz

* `docker-compose.yml`
* `Dockerfile` (backend)
* `FrontEnd/Dockerfile`

---

### 3) Levantar servicios

```bash
docker compose up -d --build
```

Servicios incluidos:

* `db` → PostgreSQL 16
* `adminer` → cliente DB web
* `backend` → API .NET
* `frontend` → Vite

---

### 4) Accesos

* **Frontend**: [http://localhost:5173](http://localhost:5173)
* **API**: [http://localhost:5000](http://localhost:5000)/swagger
* **Adminer**: [http://localhost:8081](http://localhost:8081)

  * Servidor: `db`
  * Usuario: `postgres`
  * Contraseña: `postgres`
  * Base: `PruebaTecnica2025Db`

---

### 5) Frontend `.env` en Docker

```env
VITE_API_BASE=http://localhost:5000/api
```

---

### 6) Logs y control

```bash
# logs API
docker logs -f api-pt2025

# bajar todo
docker compose down

# bajar + borrar datos DB
docker compose down -v
```

---

### 7) Notas rápidas

* Si un puerto está ocupado (5432, 5173, 8081, 5000), cambiar mapeo en `docker-compose.yml`.
* Si corrés backend **sin Docker** y frontend **con Docker**, asegurate de setear `VITE_API_BASE` al puerto de tu API local.
