import type { Middleware, DefaultState, DefaultContext, BaseRequest } from 'koa'
import type { Message } from '@bufbuild/protobuf'

type RequestHandlerContext<T> = { 
  request: { body: T }
}


declare global {
  interface ResponseData<T = any> {
    code: number
    message: string
    data?: T
  }

  type DropGrpcFields<T> = Omit<Req, '$typeName' | '$unknown'>
  type RequestHandler<Req, Res> = Middleware<{}, RequestHandlerContext<Req>, ResponseData<Res>>
  type RequestHandlerToGrpc<Req extends any, Res extends any> = RequestHandler<
    DropGrpcFields<Req>,
    Omit<DropGrpcFields<Res>, 'reached'>
  >
}