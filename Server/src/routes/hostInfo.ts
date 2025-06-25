import { ApproachRequest, ApproachResponse, GetBasicInfoResponse } from '../protoGen/hostInfo_pb'
import { checkJsonTypes, JsonType } from '../utils/checkJsonTypes'
import { DeviceManager } from '../utils/deviceManager'
import { grpcClients } from '../utils/grpcClient'
import { createResponse } from '../utils/response'

const approach: RequestHandlerToGrpc<ApproachRequest, ApproachResponse> = async (ctx, next) => {
  const body = checkJsonTypes(ctx.request.body, {
    withMacAddress: JsonType.boolean.required,
  })

  const result = await grpcClients.hostInfo.approach(body)
  ctx.body = createResponse.success(result)
  await next()
}

const getBasicInfo: RequestHandlerToGrpc<{}, GetBasicInfoResponse> = async (ctx, next) => {
  const result = await grpcClients.hostInfo.getBasicInfo({})
  ctx.body = createResponse.success(result)
  await next()
}

export const hostInfoRoute = { approach, getBasicInfo }
