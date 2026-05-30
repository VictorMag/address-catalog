# Address Catalog — Design Spec
**Date:** 2026-05-30  
**Stack:** ASP.NET Core Razor Pages + EF Core + Azure SQL

---

## Objetivo

Mostrar el listado de registros de `SalesLT.Address` (450 registros) y permitir editar los campos `City` y `StateProvince` mediante dropdowns poblados con los valores distintos de la misma tabla. Registrar `ModifiedDate` en cada edición.

---

## Base de datos

- **Servidor:** pruebadevsrv.database.windows.net  
- **BD:** testdb7  
- **Schema:** SalesLT  
- **Tabla principal:** `SalesLT.Address`

### Columnas relevantes

| Columna | Tipo | Notas |
|---------|------|-------|
| AddressID | int | PK, solo lectura |
| AddressLine1 | nvarchar(60) | Solo lectura en UI |
| City | nvarchar(30) | **Editable** via dropdown |
| StateProvince | nvarchar(50) | **Editable** via dropdown |
| CountryRegion | nvarchar(50) | Solo lectura en UI |
| PostalCode | nvarchar(15) | Solo lectura en UI |
| ModifiedDate | datetime | Se actualiza al guardar |

---

## Arquitectura

```
Pages/Addresses/Index.cshtml.cs
        ↓
Services/IAddressService  →  AddressService
        ↓
Data/AppDbContext
        ↓
Azure SQL — SalesLT.Address
```

### Estructura de carpetas

```
AddressCatalog/
├── Data/
│   ├── AppDbContext.cs
│   └── Entities/
│       └── Address.cs
├── Services/
│   ├── IAddressService.cs
│   └── AddressService.cs
├── Models/
│   └── AddressEditDto.cs
├── Pages/
│   ├── Addresses/
│   │   └── Index.cshtml (.cs)
│   └── Shared/
│       └── _Layout.cshtml
└── Program.cs
```

---

## Funcionalidades

### 1. Listado paginado
- Muestra: AddressID, AddressLine1, City, StateProvince, CountryRegion, PostalCode, ModifiedDate
- 20 registros por página
- Paginación con controles Anterior / Siguiente / número de página
- Botón "Editar" por fila

### 2. Modal de edición
- Se abre al hacer clic en "Editar"
- JS mínimo (vanilla) llena el modal con los datos de la fila: ID, City actual, StateProvince actual
- Dropdown **City**: valores DISTINCT de `SalesLT.Address.City` (269 valores)
- Dropdown **StateProvince**: valores DISTINCT de `SalesLT.Address.StateProvince` (25 valores)
- Botones: Guardar / Cancelar

### 3. Guardar cambios
- POST a `?handler=Edit` (misma página, patrón PRG)
- Servicio actualiza `City`, `StateProvince` y `ModifiedDate = DateTime.UtcNow`
- Redirect a la misma página con el número de página actual preservado

---

## Flujo del modal (PRG)

```
Clic "Editar" → JS abre modal con datos de la fila
      ↓
Usuario selecciona City y StateProvince
      ↓
POST ?handler=Edit (form submit)
      ↓
PageModel valida → llama AddressService.UpdateAsync()
      ↓
Service: UPDATE City, StateProvince, ModifiedDate WHERE AddressID = x
      ↓
Redirect → GET misma página (PRG)
      ↓
Lista se recarga con datos actualizados
```

---

## Manejo de errores

| Caso | Respuesta |
|------|-----------|
| ID no existe al editar | `NotFound()` → 404 |
| Campos vacíos | Data Annotations en DTO, validación en modal |
| Error de DB | Excepción no controlada → página de error estándar de ASP.NET |

---

## Despliegue

- **Target:** VM Windows Server (acceso RDP provisto por la empresa)
- **Método:** `dotnet publish` → copia a VM → IIS o Kestrel directo
- **Conexión a DB:** connection string en `appsettings.json` (o variable de entorno en la VM)
- El despliegue lo ejecuta el desarrollador siguiendo guía paso a paso

---

## Decisiones de diseño

- **Sin repositorio genérico**: EF Core ya actúa como repositorio. Una capa extra sería over-engineering para una entidad.
- **Sin AJAX**: El modal usa form POST estándar. Más simple, igual de funcional para este caso.
- **JS mínimo**: Solo para abrir el modal y llenar los campos. Sin librerías externas.
- **Sin autenticación**: No fue requerida en el enunciado.
