// Benchmark de throughput/latencia sobre GET /api/productos: el endpoint mas golpeado
// en la practica, ya que Caja lo llama en cada tecleo de la busqueda de productos
// (con debounce de 300ms, pero aun asi es el endpoint de lectura mas frecuente del sistema).
//
// Esto es un benchmark de capacidad (cuantas requests/seg soporta), a diferencia de
// concurrencia-ventas.js que es una prueba de CORRECCION bajo concurrencia (unicidad de NCF).
//
// Uso: API_URL=http://localhost:5080 node lectura-productos.js [duracionSegundos] [conexiones]

const autocannon = require('autocannon');

const API_URL = process.env.API_URL ?? 'http://localhost:5080';
const DURACION = Number(process.argv[2] ?? 15);
const CONEXIONES = Number(process.argv[3] ?? 20);

const ADMIN_USUARIO = 'admin';
const ADMIN_PASSWORD = 'Admin#2026';

async function main() {
  const loginRes = await fetch(`${API_URL}/api/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ nombreUsuario: ADMIN_USUARIO, password: ADMIN_PASSWORD }),
  });
  if (!loginRes.ok) throw new Error(`Login fallo: ${loginRes.status}`);
  const { token, sucursalesPermitidas } = await loginRes.json();
  const sucursalId = sucursalesPermitidas[0];

  console.log(`GET /api/productos — ${CONEXIONES} conexiones, ${DURACION}s`);

  const resultado = await autocannon({
    url: `${API_URL}/api/productos?sucursalId=${sucursalId}&texto=`,
    connections: CONEXIONES,
    duration: DURACION,
    headers: { Authorization: `Bearer ${token}` },
  });

  console.log(autocannon.printResult(resultado));

  if (resultado.non2xx > 0 || resultado.errors > 0) {
    console.error(`\nATENCION: ${resultado.non2xx} respuestas no-2xx, ${resultado.errors} errores de conexion.`);
    process.exitCode = 1;
  }
}

main().catch((err) => {
  console.error('Error en el benchmark:', err);
  process.exitCode = 1;
});
