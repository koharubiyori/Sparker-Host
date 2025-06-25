import { Middleware } from 'koa'
import { logger } from '../utils/logger'
import { __HTTP_SERVER_LOG } from '../constants'

let counter = 0

export const loggerMiddleware = ((secretPaths: string[]): Middleware => {
  return async (ctx, next) => {
    if (!__HTTP_SERVER_LOG) return next()
    counter++
    const start = Date.now()
    const body = (ctx.request as any).body
    logger.info({ 
      id: counter,
      type: 'request', 
      method: ctx.method, 
      url: ctx.url, 
      body: secretPaths.includes(ctx.path) ? '***' : body, 
      header: ctx.request.header 
    })

    await next()
    
    const ms = Date.now() - start
    logger.info({
      id: counter,
      type: 'response',
      method: ctx.method,
      url: ctx.url,
      status: ctx.status,
      ms,
      body: ctx.response.body,
      header: ctx.response.header
    })
  }
})
