import { ExceptionError } from '../middlewares/errorHandler'
import { CaptchaManager } from '../utils/captchaManager'
import { checkJsonTypes, JsonType } from '../utils/checkJsonTypes'
import { DeviceManager } from '../utils/deviceManager'
import { ErrorCode } from '../utils/errorCode'
import { createResponse } from '../utils/response'
import { Tokener } from '../utils/tokener'
import { toast } from '../utils/toast'
import { getI18n } from '../languages'
import { __DEV } from '../constants'
import { grpcClients } from '../utils/grpcClient'

interface GetPairingCodeReq {
  sessionId: string
}

interface GetPairingCodeRes {}

const getPairingCode: RequestHandler<GetPairingCodeReq, GetPairingCodeRes> = async (ctx, next) => {
  const body = checkJsonTypes(ctx.request.body, {
    sessionId: JsonType.string.required
  })
  
  const captcha = CaptchaManager.get(body.sessionId)
  const i18n = await getI18n()
  toast(i18n.f_DevicePairingCode(captcha.code), i18n.s_devicePairCodeDesc)
  
  ctx.body = createResponse.success(__DEV ? captcha.code : null)
  await next()
}

interface PairReq {
  deviceId: string
  pairingCode: string,
  sessionId: string
  username: string
  password: string
}

interface PairRes {
  token: string
}

const pair: RequestHandler<PairReq, PairRes> = async (ctx, next) => {
  const body = checkJsonTypes(ctx.request.body, {
    deviceId: JsonType.string.required,
    pairingCode: JsonType.string.required,
    sessionId: JsonType.string.required,
    username: JsonType.string.required,
    password: JsonType.string.required
  })

  const { result } = await grpcClients.hostInfo.isValidCredential({
    username: body.username,
    password: body.password
  })

  if (result !== 0) {
    throw new ExceptionError({
      code: ErrorCode.DEVICE_USERNAME_OR_PASSWORD_INVALID,
      message: 'Invalid username or password'
    })
  }

  if (CaptchaManager.validate(body.sessionId, { code: body.pairingCode })) {
    const token = await Tokener.sign({ 
      deviceId: body.deviceId,
      username: body.username,
      password: body.password
    })
    DeviceManager.addDevice({ id: body.deviceId })
    ctx.body = createResponse.success({ token: token })
  } else {
    throw new ExceptionError({ code: ErrorCode.DEVICE_INVALID_PAIRING_CODE, message: 'Invalid pairing code' })
  }

  await next()
}

interface UnpairReq {
  deviceId: string
}

interface UnpairRes {}

const unpair: RequestHandler<UnpairReq, UnpairRes> = async (ctx, next) => {
  const body = checkJsonTypes(ctx.request.body, {
    deviceId: JsonType.string.required
  })

  if (!DeviceManager.exists(body.deviceId)) {
    throw new ExceptionError({
      code: ErrorCode.DEVICE_NOT_PAIRED,
      message: 'Device not paired'
    })
  }

  await DeviceManager.deleteDevice(body.deviceId)
  ctx.body = createResponse.success()
}

// interface UpdateReq {
//   deviceId: string
//   deviceName: string
// }

// interface UpdateRes {}

// const update: RequestHandler<UpdateReq, UpdateRes> = async (ctx, next) => {
//   const body = checkJsonTypes(ctx.request.body, {
//     deviceId: JsonType.string,
//     deviceName: JsonType.string
//   })

//   if (!DeviceManager.exists(body.deviceId)) {
//     throw new ExceptionError({
//       code: ErrorCode.DEVICE_NOT_PAIRED,
//       message: 'Device not paired'
//     })
//   }

//   await DeviceManager.update(body.deviceId, { name: body.deviceName })
//   ctx.body = createResponse.success()
// }

export const deviceRoute = { getPairingCode, pair, unpair }