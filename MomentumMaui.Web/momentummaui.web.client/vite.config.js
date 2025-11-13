import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import { fileURLToPath, URL } from 'node:url';

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [react()],
    resolve: {
        alias: {
            // This line sets up the '@' alias to point to the 'src' directory
            // using the modern ES module syntax.
            '@': fileURLToPath(new URL('./src', import.meta.url))
        },
    },
    server: {
        // This is part of the default template and helps with proxying API requests.
        proxy: {
            '^/weatherforecast': {
                target: 'https://localhost:7123/',
                secure: false
            }
        },
        port: 5173
    }
})