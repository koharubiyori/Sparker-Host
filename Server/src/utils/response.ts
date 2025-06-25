import { ErrorCode } from './errorCode'

export function createResponse<T = any>(code: ErrorCode, message: string, data: T): ResponseData<T> {
  delete (data as any)?.$typeName   // Remove protobuf specific properties
  delete (data as any)?.$unknown
  return { code, message, data }
}

createResponse.success = <T = any>(data: T | null = null): ResponseData => {
  return createResponse(ErrorCode.SUCCESS, 'success', data)
}