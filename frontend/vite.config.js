import { defineConfig } from 'vite'
import path from 'path'

export default defineConfig({
    build: {
        // 打包後的成果丟到 wwwroot/dist
        outDir: path.resolve(__dirname, '../wwwroot/dist'),
        emptyOutDir: true,
        rollupOptions: {
            input: {
                // 關鍵：將入口指向你新蓋好的 common/main.js
                'common-bundle': path.resolve(__dirname, 'src/common/main.js')
            },
            output: {
                entryFileNames: 'assets/[name].js',
                assetFileNames: 'assets/[name].[ext]'
            }
        }
    }
})