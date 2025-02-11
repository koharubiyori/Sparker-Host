import { app } from 'electron'
import got, { Options, RequestError } from 'got'
import { DEFAULT_SERVICE_PORT } from '~/constants'
import infoApi, { APPROACH } from '~/main/api/info'
import { globalI18n } from '~/utils/i18n'
import logger from '~/main/utils/logger'
import { showErrorBox, showNotification } from '~/main/utils/utils'
import gotLogger from './logger'
import stringify from 'json-stringify-pretty-compact'

const serviceRequest = got.extend({
  timeout: 1000,
  prefixUrl: 'http://127.0.0.1:' + DEFAULT_SERVICE_PORT,  // Without this, Invalid URL will be thrown
  method: 'POST',
  hooks: {
    beforeRequest: [setActiveServiceUrl, gotLogger.request],
    afterResponse: [gotLogger.response],
    beforeError: [errorHook]
  }
})

export default serviceRequest

function errorHook(error: RequestError) {
  if (error.options.headers?.[APPROACH] === APPROACH) { return }
  const i18n = globalI18n()
  logger.error(error.message)
  error.options.json && logger.error(stringify(error.options.json))
  showErrorBox(i18n.s_serviceFatalError)

  app.quit()
  return error
}

async function setActiveServiceUrl(options: Options) {
  if (options.headers?.[APPROACH] === APPROACH) { return }
  options.prefixUrl = `http://127.0.0.1:${await activePort}/`   // prefixUrl of got.extend will add a slash to the tail, but it will not here
}

let activePort: Promise<number>

export function refreshActivePort() {
  activePort = getActiveServicePort()
}

async function getActiveServicePort() {
  try {
    infoApi.approach(DEFAULT_SERVICE_PORT)
    return DEFAULT_SERVICE_PORT
  } catch(e) {
    logger.warn(`Failed to access ${DEFAULT_SERVICE_PORT} port`)
    const result = await Promise.race(
      new Array(5).fill(0).map((_, index) => new Promise<number>(resolve => {
        const currentPort = DEFAULT_SERVICE_PORT + index + 1
        infoApi.approach(currentPort)
          .then(() => resolve(currentPort))
          .catch(() => logger.warn(`Failed to access ${DEFAULT_SERVICE_PORT} port`))
      }))
      .concat([
        new Promise((_, reject) => setTimeout(() => reject(new Error('Host service is not running!')), 3000))
      ])
    )

    return result
  }
}