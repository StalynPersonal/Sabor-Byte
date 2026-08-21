# Sabor Byte

Sistema de facturación / punto de venta (POS) para restaurantes, multi-sucursal, con comandas en tiempo real entre mesero, cocina y caja, control de inventario por receta, cuadre de caja por turnos, y facturación electrónica (e-CF) para la **DGII de República Dominicana**.

## Arquitectura

Backend en **Arquitectura Limpia** (Clean Architecture, sin CQRS) sobre **.NET 10**, frontend en **Blazor WebAssembly + MudBlazor** (auto-hospedado vía NuGet, sin CDN).

```
/Dominio                 → SaborByte.Dominio (entidades, reglas de negocio puras)
/Aplicacion               → SaborByte.Aplicacion (casos de uso, DTOs, validadores)
/Infraestructura          → SaborByte.Infraestructura (EF Core, SQL Server, adaptadores)
/Api                       → SaborByte.Api (host ASP.NET Core, JWT, Rate Limiting, SignalR)
/FacturacionElectronica    → FacturacionElectronicaDGII (librería DLL independiente y reutilizable)
/Web                       → Frontend (Blazor WebAssembly)
  /Apps
    /SaborByte.Web.Central    → administración, catálogos, reportes (PWA)
    /SaborByte.Web.Caja       → punto de venta / facturación (sin PWA, equipo fijo)
    /SaborByte.Web.Cocina     → KDS (Kitchen Display System), módulo opcional (PWA)
    /SaborByte.Web.Mesero     → toma de pedidos en mesa, módulo opcional (PWA)
  /Compartido
    /SaborByte.Web.UI         → componentes MudBlazor reutilizables
    /SaborByte.Web.Api        → cliente HTTP/SignalR y tipos compartidos
```

Las 4 apps requieren conexión activa al backend central — no hay sincronización offline en v1 (se evaluará como mejora futura si los locales reportan cortes de red frecuentes).

El plan de arquitectura completo (modelo de datos, flujos de comanda, integración DGII, cuadre de caja, roadmap por fases) vive en el documento de planificación del proyecto.

## Requisitos

- .NET SDK 10
- SQL Server (local o remoto)

## Backend

```bash
dotnet restore
dotnet build
dotnet run --project Api
```

La cadena de conexión y la clave JWT de desarrollo están en `Api/appsettings.Development.json` (no usar esos valores en producción — en producción deben resguardarse en un vault de secretos).

## Frontend

```bash
dotnet run --project Web/Apps/SaborByte.Web.Central
dotnet run --project Web/Apps/SaborByte.Web.Caja
dotnet run --project Web/Apps/SaborByte.Web.Cocina
dotnet run --project Web/Apps/SaborByte.Web.Mesero
```

MudBlazor se instala vía paquete NuGet (auto-hospedado) en cada app — sus assets se sirven desde `_content/MudBlazor/...` sin depender de un CDN externo.

## Facturación Electrónica (e-CF / DGII)

El proyecto `/FacturacionElectronica` es una librería independiente y reutilizable (sin dependencias del resto del sistema) para la generación, validación, firma y envío de comprobantes electrónicos ante la **DGII (Dirección General de Impuestos Internos) de República Dominicana**. Se invoca solo cuando una sucursal tiene la facturación electrónica activada.

## Estado del proyecto

**Funcional de punta a punta** (probado contra SQL Server real):

- ✅ Autenticación JWT con roles y sucursales asignadas por usuario.
- ✅ Caja: apertura/cierre de turno (con validación de IP/hostname autorizado y turno único abierto), búsqueda de producto, venta con ITBIS/descuento/propina, NCF tradicional cuando aplica.
- ✅ Comandas Mesero → Cocina → Caja en tiempo real vía SignalR, con cancelación y reverso automático de inventario.
- ✅ Inventario por receta (BOM): descuenta insumos automáticamente al vender/enviar a cocina.
- ✅ Clientes, Cuentas por Cobrar/Pagar.
- ✅ Reportes de ventas consolidados multi-sucursal, gestión de usuarios (Admin), health checks en `/health`.
- ✅ Autorización de supervisor para descuentos (código de un solo uso) + auditoría de acciones sensibles.
- ✅ SMTP opcional por sucursal.
- 🟡 e-CF/DGII: generación y firma XML del e-CF 32 basadas en los XSD oficiales, con pruebas automatizadas — **el envío real a los servicios web de DGII no está implementado** (no hay acceso verificado a esas URLs/contrato en este proyecto); las ventas quedan en estado "Contingencia" sin bloquear la operación.
- ⬜ Pendiente: promociones/combos, reportes gerenciales avanzados, cobertura de pruebas más amplia (solo la librería de facturación electrónica tiene pruebas hoy), revisión de seguridad formal.

Hay un pipeline de CI en GitHub Actions (`.github/workflows/ci.yml`) que compila el proyecto y corre las pruebas de `FacturacionElectronicaDGII.Tests` en cada push/PR a `main`.
