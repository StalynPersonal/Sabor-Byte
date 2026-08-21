# Pruebas E2E (Playwright)

Prueba del flujo dorado del sistema: login → crear producto (Central) → abrir turno →
vender → cerrar turno (Caja). Corre contra el stack real (Api + SQL Server + los
proyectos Blazor WebAssembly), no contra mocks.

## Requisitos

- Node.js (ya usado para instalar Playwright).
- SQL Server local con la base `SaborByteDb` migrada (la API la migra sola al arrancar).
- Haber corrido `npm install` y `npx playwright install chromium` una vez en esta carpeta.

Si `npm`/`npx` fallan con `UNABLE_TO_GET_ISSUER_CERT_LOCALLY` (típico detrás del
proxy TLS de Kaspersky en este equipo), usar:

```bash
npm install -D @playwright/test --strict-ssl=false
NODE_TLS_REJECT_UNAUTHORIZED=0 npx playwright install chromium
```

## Cómo correr las pruebas

1. Levantar la API en el puerto 5080 (los `appsettings.json` del frontend apuntan ahí):

   ```bash
   dotnet run --project ../Api --urls http://localhost:5080
   ```

2. Levantar Central y Caja (en otras terminales, puertos libres a elección):

   ```bash
   dotnet run --project ../Web/Apps/SaborByte.Web.Central --urls http://localhost:5140
   dotnet run --project ../Web/Apps/SaborByte.Web.Caja --urls http://localhost:5141
   ```

3. Correr las pruebas:

   ```bash
   CENTRAL_URL=http://localhost:5140 CAJA_URL=http://localhost:5141 npm test
   ```

   (`CENTRAL_URL`/`CAJA_URL` son opcionales si se usan los puertos por defecto del
   ejemplo arriba — ver `tests/flujo-caja.spec.ts`.)

4. Ver el reporte HTML de la última corrida: `npm run report`.

## Usuario de prueba

Se usa el usuario `admin` sembrado por `Infraestructura/Persistencia/SeedData.cs`
(contraseña `Admin#2026`), que ya tiene acceso a la sucursal y caja "01" sembradas
por defecto. No crea usuarios nuevos.

## Notas de diseño de las pruebas

- **No se usa `page.goto()` para navegar entre páginas ya autenticado**: la sesión
  (`SesionCliente`) vive solo en memoria, sin `localStorage` (decisión de arquitectura
  para no persistir tokens en el navegador) — una navegación de página completa la
  pierde. Las pruebas navegan haciendo clic en los links del menú, como lo haría un
  usuario real.
- El ticket impreso usa la clase `.solo-imprimir` (`display:none` en pantalla, visible
  solo vía `@media print`), así que la prueba valida que el ticket exista en el DOM,
  no que sea visible.
- Un turno de caja abierto y no cerrado por una corrida anterior (por ejemplo, si la
  prueba se interrumpió a mitad de camino) bloqueará la siguiente corrida con "Ya
  existe un turno abierto en esta caja." — hay que cerrarlo manualmente (UI o SQL)
  antes de reintentar.
