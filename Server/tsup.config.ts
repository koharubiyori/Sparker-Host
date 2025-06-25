import { defineConfig } from 'tsup'

export default defineConfig({
  entry: ['src/server.ts'],
  format: ['cjs'],
  target: 'node24',
  clean: true,
  splitting: false,
  dts: true,
  minify: true,
  noExternal: [/./],
  define: {
    'process.env.NODE_ENV': '"production"',
  }
})
