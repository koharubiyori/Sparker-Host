
import type { Configuration, WebpackPluginInstance } from 'webpack'
import { EnvironmentPlugin } from 'webpack'
import path from 'path'
import { rules } from './webpack.rules'
import { plugins } from './webpack.plugins'

const webpackConfig: Configuration = {
  /**
   * This is the main entry point for your application, it's the first file
   * that runs in the main process.
   */
  entry: './src/index.ts',
  // Put your normal webpack config below here
  module: {
    rules
  },
  plugins: (plugins as WebpackPluginInstance[]).concat([
    new EnvironmentPlugin({
      NODE_ENV: process.env.NODE_ENV,
    })
  ]),
  resolve: {
    extensions: ['.js', '.ts', '.jsx', '.tsx', 'mts', '.css', '.json'],
    alias: {
      '~': path.resolve(__dirname, '../src')
    }
  },
  optimization: {
    minimize: true
  },
}

export default webpackConfig