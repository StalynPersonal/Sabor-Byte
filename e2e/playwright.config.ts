import { defineConfig, devices } from '@playwright/test';

// Las apps se levantan manualmente antes de correr las pruebas (API + Central + Caja),
// ver README.md. Cada test resuelve su propia baseURL segun la app que ejercita.
export default defineConfig({
  testDir: './tests',
  fullyParallel: false,
  workers: 1,
  retries: 0,
  reporter: [['html', { open: 'never' }], ['list']],
  use: {
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
});
