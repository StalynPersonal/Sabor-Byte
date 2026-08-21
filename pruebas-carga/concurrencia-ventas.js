// Prueba de carga / concurrencia sobre el endpoint critico del sistema: POST /api/ventas.
//
// El riesgo real (documentado en el plan de arquitectura, seccion 4, paso 2) es que dos
// ventas concurrentes en la misma sucursal reciban el MISMO numero de NCF si el incremento
// de SecuenciaProxima no es realmente atomico. Un NCF duplicado es un problema fiscal grave
// (dos comprobantes distintos con el mismo numero), asi que esta prueba no solo mide
// throughput/latencia: verifica explicitamente que, bajo N ventas simultaneas, cada NCF
// emitido sea unico.
//
// Requiere: Api corriendo (por defecto en http://localhost:5080) con SQL Server accesible,
// y sqlcmd en el PATH para el paso de siembra de una secuencia NCF de prueba.
//
// Uso:
//   node concurrencia-ventas.js [concurrencia]
//   API_URL=http://localhost:5080 node concurrencia-ventas.js 50

const { execSync } = require('node:child_process');

const API_URL = process.env.API_URL ?? 'http://localhost:5080';
const CONCURRENCIA = Number(process.argv[2] ?? process.env.CONCURRENCIA ?? 30);

const ADMIN_USUARIO = 'admin';
const ADMIN_PASSWORD = 'Admin#2026';

async function loginAsync() {
  const res = await fetch(`${API_URL}/api/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ nombreUsuario: ADMIN_USUARIO, password: ADMIN_PASSWORD }),
  });
  if (!res.ok) throw new Error(`Login fallo: ${res.status} ${await res.text()}`);
  const data = await res.json();
  return data; // { token, sucursalesPermitidas, ... } segun LoginResponseDto
}

function sqlcmd(query) {
  const cmd = `sqlcmd -S localhost -d SaborByteDb -E -C -h -1 -W -Q "SET NOCOUNT ON; SET QUOTED_IDENTIFIER ON; ${query.replace(/"/g, '\\"')}"`;
  return execSync(cmd, { encoding: 'utf-8' }).trim();
}

async function main() {
  console.log(`API: ${API_URL} | concurrencia: ${CONCURRENCIA} ventas simultaneas`);

  const login = await loginAsync();
  const token = login.token;
  const sucursalId = login.sucursalesPermitidas[0];
  const authHeaders = { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' };

  // 1) Sembrar una secuencia NCF de prueba (si no hay ninguna activa, las ventas
  //    salen "sin NCF" y la prueba pierde su proposito). Rango amplio para que
  //    ninguna corrida se quede sin numeros disponibles.
  console.log('Sembrando secuencia NCF de prueba...');
  sqlcmd(`
    DELETE FROM facturacion.SecuenciasNcf WHERE SucursalId = '${sucursalId}' AND TipoComprobante = '99';
    INSERT INTO facturacion.SecuenciasNcf (Id, SucursalId, Serie, TipoComprobante, SecuenciaInicial, SecuenciaProxima, SecuenciaFinal, FechaVencimiento, FechaRegistro, Activa)
    VALUES (NEWID(), '${sucursalId}', 'E', '99', 1, 1, 999999, DATEADD(YEAR, 1, GETUTCDATE()), GETUTCDATE(), 1);
  `);

  // 2) Producto de prueba (vendible, precio fijo) via la API para no depender del catalogo real.
  console.log('Creando producto de prueba...');
  const productoRes = await fetch(`${API_URL}/api/productos?sucursalId=${sucursalId}`, {
    method: 'POST',
    headers: authHeaders,
    body: JSON.stringify({ nombre: `Carga NCF ${Date.now()}`, precio: 100, tipoProducto: 1, aplicaItbis: true }),
  });
  if (!productoRes.ok) throw new Error(`Crear producto fallo: ${productoRes.status} ${await productoRes.text()}`);
  const { id: productoId } = await productoRes.json();

  // 3) Caja + turno abierto (limpia cualquier turno abierto previo de una corrida interrumpida).
  const cajasRes = await fetch(`${API_URL}/api/caja?sucursalId=${sucursalId}`, { headers: authHeaders });
  const cajas = await cajasRes.json();
  const cajaId = cajas[0].id;

  sqlcmd(`UPDATE caja.TurnosCaja SET Estado = 1 WHERE CajaId = '${cajaId}' AND Estado = 0;`);

  console.log('Abriendo turno de caja...');
  const turnoRes = await fetch(`${API_URL}/api/caja/turnos/abrir`, {
    method: 'POST',
    headers: authHeaders,
    body: JSON.stringify({ cajaId, montoAperturaEfectivo: 1000 }),
  });
  if (!turnoRes.ok) throw new Error(`Abrir turno fallo: ${turnoRes.status} ${await turnoRes.text()}`);
  const { turnoCajaId } = await turnoRes.json();

  // 4) Disparar N ventas concurrentes del mismo producto, en el mismo turno/sucursal —
  //    exactamente el escenario de riesgo para la secuencia NCF.
  console.log(`Disparando ${CONCURRENCIA} ventas concurrentes...`);
  const inicio = Date.now();

  const promesas = Array.from({ length: CONCURRENCIA }, async () => {
    const t0 = Date.now();
    const res = await fetch(`${API_URL}/api/ventas?sucursalId=${sucursalId}`, {
      method: 'POST',
      headers: authHeaders,
      body: JSON.stringify({
        turnoCajaId,
        formaPago: 0,
        items: [{ productoId, cantidad: 1 }],
      }),
    });
    const ms = Date.now() - t0;
    if (!res.ok) {
      return { ok: false, ms, error: `${res.status} ${await res.text()}` };
    }
    const data = await res.json();
    return { ok: true, ms, numeroNcf: data.numeroNcf };
  });

  const resultados = await Promise.all(promesas);
  const totalMs = Date.now() - inicio;

  // 5) Reporte + la verificacion que de verdad importa: unicidad de NCF.
  const exitosos = resultados.filter((r) => r.ok);
  const fallidos = resultados.filter((r) => !r.ok);
  const ncfs = exitosos.map((r) => r.numeroNcf);
  const ncfsUnicos = new Set(ncfs);
  const duplicados = ncfs.filter((n, i) => ncfs.indexOf(n) !== i);

  const latencias = exitosos.map((r) => r.ms).sort((a, b) => a - b);
  const percentil = (p) => latencias[Math.floor((latencias.length - 1) * p)] ?? 0;

  console.log('\n--- Resultado ---');
  console.log(`Total: ${resultados.length} | exitosas: ${exitosos.length} | fallidas: ${fallidos.length}`);
  console.log(`Tiempo total (todas en paralelo): ${totalMs} ms`);
  if (latencias.length > 0) {
    console.log(`Latencia por venta — p50: ${percentil(0.5)} ms | p95: ${percentil(0.95)} ms | max: ${latencias.at(-1)} ms`);
  }
  console.log(`NCFs emitidos: ${ncfs.length} | NCFs unicos: ${ncfsUnicos.size}`);

  if (fallidos.length > 0) {
    console.log('\nErrores (primeros 5):');
    fallidos.slice(0, 5).forEach((f) => console.log(`  - ${f.error}`));
  }

  // 6) Limpieza: cerrar el turno para no bloquear la siguiente corrida.
  await fetch(`${API_URL}/api/caja/turnos/cerrar`, {
    method: 'POST',
    headers: authHeaders,
    body: JSON.stringify({ turnoCajaId, denominaciones: [] }),
  });

  if (duplicados.length > 0) {
    console.error(`\nFALLO: se encontraron NCF duplicados bajo concurrencia: ${[...new Set(duplicados)].join(', ')}`);
    process.exitCode = 1;
    return;
  }

  if (fallidos.length > 0) {
    console.error(`\nFALLO: ${fallidos.length} ventas fallaron.`);
    process.exitCode = 1;
    return;
  }

  console.log('\nOK: todas las ventas concurrentes tuvieron exito y ningun NCF se repitio.');
}

main().catch((err) => {
  console.error('Error en la prueba de carga:', err);
  process.exitCode = 1;
});
