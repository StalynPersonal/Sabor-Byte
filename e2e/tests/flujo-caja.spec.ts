import { test, expect } from '@playwright/test';

// Flujo dorado end-to-end (ver seccion "Verificacion / Proximos pasos" del plan de arquitectura):
// login -> crear producto en Central -> abrir caja -> vender -> cerrar caja.
// Requiere tener corriendo: Api, Central y Caja (ver README.md de esta carpeta).
const CENTRAL_URL = process.env.CENTRAL_URL ?? 'http://localhost:5140';
const CAJA_URL = process.env.CAJA_URL ?? 'http://localhost:5141';

const ADMIN_USUARIO = 'admin';
const ADMIN_PASSWORD = 'Admin#2026';

// Nombre unico por corrida para no chocar con productos de ejecuciones anteriores.
const NOMBRE_PRODUCTO = `E2E Hamburguesa ${Date.now()}`;

test.describe.serial('Flujo dorado: login -> abrir caja -> vender -> cerrar caja', () => {
  test('crea un producto vendible desde Central', async ({ page }) => {
    await page.goto(`${CENTRAL_URL}/login`);
    await page.getByLabel('Usuario').fill(ADMIN_USUARIO);
    await page.getByLabel('Contraseña').fill(ADMIN_PASSWORD);
    await page.getByRole('button', { name: 'Entrar' }).click();

    // Espera a que el login termine y se renderice el layout autenticado (Home)
    // antes de navegar, para no hacer click en el link mientras aun se resuelve el login.
    await expect(page.getByText('Módulo de administración')).toBeVisible();

    // Navegacion dentro de la SPA (no page.goto): la sesion vive solo en memoria
    // (sin localStorage, decision de diseno), asi que una recarga completa la pierde.
    await page.getByRole('link', { name: 'Productos' }).click();
    await page.waitForURL(/\/productos$/);
    await expect(page.getByRole('heading', { name: 'Productos' })).toBeVisible();

    await page.getByRole('button', { name: 'Nuevo producto' }).click();
    await page.getByLabel('Nombre').fill(NOMBRE_PRODUCTO);
    await page.getByLabel('Precio').fill('250');
    await page.getByRole('button', { name: 'Guardar' }).click();

    await expect(page.getByText(NOMBRE_PRODUCTO)).toBeVisible();
  });

  test('abre turno, vende el producto y cierra turno en Caja', async ({ page }) => {
    await page.goto(`${CAJA_URL}/login`);
    await page.getByLabel('Usuario').fill(ADMIN_USUARIO);
    await page.getByLabel('Contraseña').fill(ADMIN_PASSWORD);
    await page.getByRole('button', { name: 'Entrar' }).click();

    await expect(page.getByRole('heading', { name: 'Abrir turno de caja' })).toBeVisible();
    await page.getByLabel('Monto de apertura (efectivo)').fill('1000');
    await page.getByRole('button', { name: 'Abrir turno' }).click();

    const buscador = page.getByLabel('Buscar producto (código de barra o descripción)');
    await expect(buscador).toBeVisible();
    await buscador.fill(NOMBRE_PRODUCTO);
    await page.waitForTimeout(400); // debounce de busqueda (300ms)

    await page.getByText(NOMBRE_PRODUCTO).click();
    await expect(page.getByText(`1 x ${NOMBRE_PRODUCTO}`)).toBeVisible();

    await page.getByRole('button', { name: 'Facturar' }).click();
    // El ticket usa la clase .solo-imprimir (display:none en pantalla, solo visible
    // via @media print), asi que se valida que exista en el DOM, no que sea visible.
    await expect(page.getByText('¡Gracias por su compra!')).toBeAttached();

    // El carrito debe quedar vacio tras facturar (el ticket impreso, oculto en pantalla,
    // conserva su propia copia de la linea, por eso se valida el subtotal en vez del texto).
    await expect(page.getByText('Subtotal: RD$ 0.00')).toBeVisible();

    await page.getByRole('button', { name: 'Cerrar turno' }).click();
    await expect(page.getByText('Cierre de turno — conteo de efectivo')).toBeVisible();
    await page.getByRole('button', { name: 'Confirmar cierre' }).click();

    await expect(page.getByRole('heading', { name: 'Abrir turno de caja' })).toBeVisible();
  });
});
