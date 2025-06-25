import dayjs from 'dayjs'
import p from 'path'
import winston, { format, PredefinedMeta } from 'winston'
import { __DEV, LOG_PATH, __OUTPUT_ALL_LOGS } from '../constants'

const myFormat = format.printf(({ level, message, ...meta }) => {
  const formattedTimestamp = meta.timestamp
    ? dayjs(meta.timestamp as string).format('HH:mm:ss ')
    : ''
  const predefinedMeta: PredefinedMeta = meta
  const label = (() => {
    if (typeof predefinedMeta.label === 'string') return ` [${predefinedMeta.label}]`
    if (Array.isArray(predefinedMeta.label)) return ' ' + predefinedMeta.label.map(item => `[${item}]`).join(' ')
    return ''
  })()
  const error = predefinedMeta instanceof Error ? predefinedMeta : predefinedMeta.error
  const errorMessage = error ? `${error}\n${error.stack}` : ''
  return `${formattedTimestamp}[${level}]${label} ${message}${errorMessage}`
})

export let logger: winston.Logger

export async function initializeLogger() {
  const currentTimestamp = Math.floor(Date.now() / 1000 / 60)
  
  logger = winston.createLogger({
  level: 'info',
  transports: [
    new winston.transports.File({
      level: 'error',
      filename: p.join(LOG_PATH, `server_error_${currentTimestamp}.log`),
      format: format.combine(format.timestamp(), format.json())
    }),
    ...(__DEV || __OUTPUT_ALL_LOGS ? [
      new winston.transports.Console({
        level: 'debug',
        format: format.combine(
          format.colorize({ all: true }),
          format.timestamp(),
          myFormat
        )
      }),
      new winston.transports.File({
        level: 'debug',
        filename: p.join(LOG_PATH, `server_combined_${currentTimestamp}.log`),
        format: format.combine(format.timestamp(), format.json())
      })
    ] : [])
  ]
})
}
