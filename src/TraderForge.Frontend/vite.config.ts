import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

export default defineConfig({
  plugins: [
    react(),
    tailwindcss(),
  ],
  resolve: {
    alias: {
      '@api': '/src/api',
      '@components': '/src/components',
      '@hooks': '/src/hooks',
      '@pages': '/src/pages',
      '@router': '/src/router',
      '@store': '/src/store',
      '@models': '/src/models',
      '@utils': '/src/utils'
    }
  },
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5116',
        changeOrigin: true,
      },
      '/hubs': {
        target: 'http://localhost:5116',
        changeOrigin: true,
        ws: true,
      },
    },
  },
})
