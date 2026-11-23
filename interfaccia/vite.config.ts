import react from '@vitejs/plugin-react';
import { defineConfig } from 'vitest/config';

// In sviluppo il proxy toglie Origin: il filtro locale dell'host resta invariato.
const HOST_PROTOTIPO = 'http://127.0.0.1:5180';

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: HOST_PROTOTIPO,
        configure: (proxy) => {
          proxy.on('proxyReq', (richiesta) => richiesta.removeHeader('origin'));
        },
      },
    },
  },
  build: { outDir: 'dist', emptyOutDir: true },
  test: {
    environment: 'jsdom',
    globals: true,
    setupFiles: ['./src/prova/setup.ts'],
    include: ['src/**/*.test.{ts,tsx}'],
  },
});
