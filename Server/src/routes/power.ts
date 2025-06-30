import { IsLockedResponse, ShutdownRequest, SleepRequest, UnlockResponse } from '../protoGen/power_pb'
import { checkJsonTypes, JsonType } from '../utils/checkJsonTypes'
import { grpcClients } from '../utils/grpcClient'
import { createResponse } from '../utils/response'
import { Tokener } from '../utils/tokener'

const shutdown: RequestHandlerToGrpc<ShutdownRequest, {}> = async (ctx, next) => {
  const body = checkJsonTypes(ctx.request.body, {
    force: JsonType.boolean,
    hybrid: JsonType.boolean,
    reboot: JsonType.boolean,
    timeout: JsonType.number
  })
  
  await grpcClients.power.shutdown(body)
  ctx.body = createResponse.success()
  await next()
}

const sleep: RequestHandlerToGrpc<SleepRequest, {}> = async (ctx, next) => {
  const body = checkJsonTypes(ctx.request.body, {
    hibernate: JsonType.boolean.required
  })
  
  await grpcClients.power.sleep(body)
  ctx.body = createResponse.success()
  await next()
}

const unlock: RequestHandlerToGrpc<{}, UnlockResponse> = async (ctx, next) => {
  const token = ctx.request.headers['authorization']!!
  const { username, password, domain } = await Tokener.decode(token)
  ctx.body = createResponse.success(await grpcClients.power.unlock({ username, domain, password }))
  await next()
}

const lock: RequestHandlerToGrpc<{}, {}> = async (ctx, next) => {
  await grpcClients.userUtil.lockSession({})
  ctx.body = createResponse.success()
  await next()
}

const isLocked: RequestHandlerToGrpc<{}, IsLockedResponse> = async(ctx, next) => {
  ctx.body = createResponse.success(await grpcClients.power.isLocked({}))
  await next() 
}

export const powerRoute = { shutdown, sleep, unlock, lock, isLocked }