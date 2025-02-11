import stringify from "json-stringify-pretty-compact"
import { AfterResponseHook, BeforeRequestHook } from 'got'
import logger from '~/main/utils/logger'

function reqLog(type: 'request' | 'response', url: string, params: any, body: any, ...more: any[]) {
  const title = type.toUpperCase()
  const bgColor = type === 'request' ? 'green' : 'blueviolet'
  const divider = '------'
  const headSpace = ' '.repeat(divider.length + 1)
  // For stringify(), the indent will have not effect if the length of characters is shorter than the default value 80 in a line
  const jsonHeadIndent = headSpace.length + 6 

  logger.http(`${divider} START ${title}: ${url} ${divider}`)
    params !== undefined && type === 'request' && logger.http(`${headSpace}Request Parameters: ${stringify(params, { indent: jsonHeadIndent })}`)
    body !== undefined && logger.http(`${headSpace}Body: ${stringify(body, { indent: jsonHeadIndent })}`)
  logger.http(`${divider} END ${title} ${divider}`)
  logger.http('')
  
  if (process.type === 'renderer') {
    console.group()
      console.debug(`%c${title}%c: ${url}`, `background-color: ${bgColor}; color: white`, 'background-color:transparent; color:black')
      body !== undefined && console.debug('Request Parameters:', body)
      console.debug(...more)
    console.groupEnd()
  }
}

const requestLogger: BeforeRequestHook = (options) => {
  const urlObject = options.url
  const url = urlObject.hostname + urlObject.pathname
  let queryParams: any = {}
  urlObject.searchParams.forEach((val, name) => queryParams[name] = val)

  if (options.method === 'POST') {
    reqLog('request', url, options.json, options.body ? JSON.parse(options.body as string) : options.json, options)
  } else {
    reqLog('request', url, queryParams, undefined, options)
  }
}

const responseLogger: AfterResponseHook = (res) => {
  const options = res.request.options
  const url = options.url
  const reqUrl = url.hostname + url.pathname
  let queryParams: any = {}
  url.searchParams.forEach((val, name) => queryParams[name] = val)

  const body = res.headers['content-type']?.split(';')[0] === 'application/json' ? JSON.parse(res.body as any) : res.body
  if (options.method === 'POST') {
    reqLog('response', reqUrl, queryParams, options.json, res)
  } else {
    reqLog('response', reqUrl, queryParams, undefined, res)
  }

  return res
}

const gotLogger = {
  request: requestLogger,
  response: responseLogger
}

export default gotLogger
