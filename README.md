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

En construcción — ver roadmap por fases en el documento de planificación.
