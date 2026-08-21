# Pruebas de carga

Dos scripts, con objetivos distintos, contra la Api real (SQL Server real, no mocks):

- **`concurrencia-ventas.js`** — prueba de *corrección* bajo concurrencia, no de throughput.
  Dispara N ventas simultáneas del mismo producto/turno y verifica que cada `numeroNcf`
  emitido sea único. Ver "Hallazgo" abajo.
- **`lectura-productos.js`** — benchmark de *capacidad* sobre `GET /api/productos`
  (el endpoint de lectura más frecuente: Caja lo llama en cada búsqueda de producto).

## Requisitos

- Api corriendo con SQL Server accesible (por defecto `http://localhost:5080`).
- `sqlcmd` en el PATH (usado por `concurrencia-ventas.js` para sembrar una secuencia NCF
  de prueba y limpiar turnos abiertos entre corridas).
- `npm install --strict-ssl=false` si `npm` falla con `UNABLE_TO_GET_ISSUER_CERT_LOCALLY`
  (mismo workaround del proxy TLS de Kaspersky documentado en `../e2e/README.md`).

## Uso

```bash
dotnet run --project ../Api --urls http://localhost:5080   # en otra terminal

API_URL=http://localhost:5080 node concurrencia-ventas.js 50
API_URL=http://localhost:5080 node lectura-productos.js 15 20   # 15s, 20 conexiones
```

## Hallazgo real (ya corregido): NCF duplicados bajo concurrencia

La primera corrida de `concurrencia-ventas.js` (30 ventas simultáneas) encontró un bug
real: **las 30 ventas recibieron el mismo número de NCF**. Causa: `VentaAppService`
leía `SecuenciaNcf.SecuenciaProxima`, la incrementaba en memoria, y recién la persistía
en el `SaveChangesAsync` final de `CrearVentaAsync` — dos ventas concurrentes podían leer
el mismo valor antes de que cualquiera escribiera. Para un sistema de facturación fiscal,
un NCF duplicado es un problema serio (dos comprobantes distintos con el mismo número).

Corregido en `Aplicacion/Facturacion/VentaAppService.cs` (`AsignarNcfSiAplicaAsync`) con
un patrón compare-and-swap vía `ExecuteUpdateAsync` (`UPDATE ... WHERE Id = @id AND
SecuenciaProxima = @valorLeido`): bajo concurrencia, solo una transacción logra el UPDATE
con ese valor exacto; el resto recibe 0 filas afectadas y reintenta con el valor ya
avanzado. Verificado: 50 y 100 ventas concurrentes, 100% de NCFs únicos en ambos casos
(ver sección siguiente para el detalle de la corrida con 100).

**Por qué no hay un unit test de esta regresión**: se intentó (proveedor InMemory de
`Aplicacion.Tests` no soporta `ExecuteUpdateAsync`, así que se probó con SQLite en
memoria/cache compartido), pero SQLite serializa lecturas y escrituras a nivel de toda
la base de datos, no por fila como SQL Server — bajo ese modelo la ventana de carrera
que causaba el bug original no se reproduce, y el test pasaba incluso contra el código
viejo (defectuoso). Un test que no falla contra el bug que dice cubrir es peor que no
tener test, así que se descartó; esta corrección solo queda validada por
`concurrencia-ventas.js` contra SQL Server real, que es la base de datos de producción.

## Hallazgo de capacidad: el rate limiter global es el techo real, no la base de datos

Con `lectura-productos.js` (20 conexiones, 10s) la Api sirvió ~24,000 req/s a nivel de
red/routing, pero el **rate limiter global** (`Api/Program.cs`, 60 requests/10s por IP,
aplicado a *todos* los endpoints autenticados) devolvió 429 en el 99.96% de las
peticiones — solo ~120 pasaron (2 ventanas de 60). Esto confirma que el límite real de
capacidad hoy no es el backend ni SQL Server, sino ese límite global de 60 req/10s por IP.

Esto es una configuración deliberada (mitigar abuso), no un bug, pero vale la pena que
el equipo lo revise antes de producción: **todo el tráfico de una sucursal (Caja +
Cocina + Mesero + Central) puede compartir la misma IP pública saliente** (NAT), y ese
tráfico comparte el mismo presupuesto de 60 req/10s — una búsqueda de producto en Caja
con varios cajeros activos a la vez podría toparse con el límite en horas pico. No se
modificó esta configuración (es una decisión de seguridad que le corresponde al equipo,
no algo que este análisis deba cambiar por su cuenta); queda documentado para que se
decida con datos reales de uso (ej. partición por usuario autenticado en vez de por IP
para rutas ya protegidas por `[Authorize]`, o subir el límite).

## Resultado de la corrida con 100 ventas concurrentes (post-fix)

```
Total: 100 | exitosas: 57 | fallidas: 43
NCFs emitidos: 57 | NCFs unicos: 57
Latencia por venta — p50: 185 ms | p95: 227 ms | max: 229 ms
```

Los 43 fallos son `429` del rate limiter global (ver arriba) — comportamiento esperado
bajo una ráfaga de 100 requests en <1s desde la misma IP, no una falla del sistema de
ventas. Cero NCFs duplicados entre las 57 que sí pasaron.
