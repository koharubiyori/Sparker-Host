import dayjs from 'dayjs'
import p from 'path'
import winston, { format } from 'winston'
import { LOG_PATH } from '~/constants'

const myFormat = format.printf(info => {
  const formattedTimestamp = info.timestamp ? dayjs(info.timestamp as string).format('YYYY-MM-DD HH:mm:ss ') : ''
  return `${formattedTimestamp}${info.level}: ${info.message}`
})


const logger = winston.createLogger({
  level: 'info',
  format: myFormat,
  transports: [
    new winston.transports.Console({ 
      level: 'debug', 
      format: format.combine(
        format.colorize({ all: true }),
      ) 
    }),
    new winston.transports.File({ 
      level: 'error', 
      filename: p.join(LOG_PATH, 'error.log'),
      format: format.combine(
        format.timestamp(),
      )
    }),
    new winston.transports.File({ 
      level: 'debug',
      filename: p.join(LOG_PATH, 'combined.log'),
      format: format.combine(
        format.timestamp(),
      )
    }),
  ],
})

export default logger