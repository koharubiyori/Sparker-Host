import { Middleware } from 'koa'
import { DeviceManager } from '../utils/deviceManager'
import { Tokener } from '../utils/tokener'
import { logger } from '../utils/logger'

export const authorization = ((excludePaths: string[]): Middleware => {
  return async (ctx, next) => {
    if (excludePaths.includes(ctx.path)) return next()

    const token = ctx.request.headers['authorization']
    if (!(await isAuthorized(token))) {
      ctx.status = 401
      return
    }

    await next()
  }
})

export async function isAuthorized(token?: string): Promise<boolean> {
  if (!token) return false
  try {
    const { deviceId } = await Tokener.decode(token)
    return DeviceManager.exists(deviceId)
  } catch(e) {
    logger.error('Failed to Authorize', e)
    return false
  }
}