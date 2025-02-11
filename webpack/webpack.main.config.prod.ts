import CopyPlugin from 'copy-webpack-plugin'
import type { Configuration } from 'webpack'
import merge from 'webpack-merge'
import baseWebpackConfig from './webpack.main.config.base'

const webpackConfig: Configuration = merge(baseWebpackConfig, {
  plugins: [
    new CopyPlugin({
      patterns: [
        { from: "assets", to: "assets" },
      ],
    })
  ]
})

export default webpackConfig