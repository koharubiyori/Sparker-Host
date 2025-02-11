import CopyPlugin from 'copy-webpack-plugin'
import type { Configuration } from 'webpack'
import merge from 'webpack-merge'
import baseWebpackConfig from './webpack.main.config.base'

const webpackConfig: Configuration = merge(baseWebpackConfig, {
  devtool: 'source-map',
  mode: 'development',
  target: 'electron-main',
  

  plugins: [
    // new CopyPlugin({
    //   patterns: [
    //     { from: "resources", to: "resources" },
    //   ],
    // })
  ]
})

export default webpackConfig