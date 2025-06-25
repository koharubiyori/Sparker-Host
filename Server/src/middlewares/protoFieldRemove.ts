import { Middleware } from 'koa'

// Remove protobuf specific fields from the response body
export const protoFieldRemover: Middleware = async (ctx, next) => {
  await next()
  delete ctx.body.data?.$typeName
  delete ctx.body.data?.$unknown
}