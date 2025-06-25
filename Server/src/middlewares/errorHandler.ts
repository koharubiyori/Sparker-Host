import type { Context, Next } from 'koa'
import { logger } from '../utils/logger'
import { ErrorCode } from '../utils/errorCode'
import { ConnectError } from '@connectrpc/connect'

export interface ExceptionErrorOptions {
  message?: string
  status?: number
  code?: ErrorCode
  data?: any
} 

export class ExceptionError extends Error {
  public code: ErrorCode
  public data: any
  
  constructor({
    message = 'Unknown error',
    code = ErrorCode.UNKNOWN_ERROR,
    data = null,
  }: ExceptionErrorOptions) {
    super(message)
    this.name = 'ExceptionError'
    this.code = code
    this.data = data
  }
}

export async function errorHandler(ctx: Context, next: Next) {
  try {
    await next()
  } catch (err: any) {
    if (err instanceof ExceptionError) {
      logger.error('ExceptionError:', err)
      ctx.body = {
        code: err.code,
        message: err.message,
        data: err.data,
      }
      // ctx.app.emit('error', err, ctx)
      return
    }

    if (err instanceof ConnectError) {
      logger.error('Grpc ConnectError:', err)
      ctx.body = {
        code: ErrorCode.GRPC_CONNECT_ERROR,
        message: 'Grpc Connect Error',
        data: null,
      }
      return
    }
    
    ctx.status = 500
    ctx.body = {
      code: ErrorCode.INTERNAL_ERROR,
      message: 'Internal Server Error',
      data: null,
    }
    
    logger.error('Global error:', err)
    throw err
  }
}
