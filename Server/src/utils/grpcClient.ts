import { DescService } from '@bufbuild/protobuf'
import { Client, createClient, Transport } from '@connectrpc/connect'
import { createGrpcTransport } from '@connectrpc/connect-node'
import { HostInfoService } from '../protoGen/hostInfo_pb'
import { PowerService } from '../protoGen/power_pb'
import { DesktopService } from '../protoGen/desktop_pb'

export type GrpcClientType = 'system' | 'user'

export function updateClientsWithPort(type: GrpcClientType, port: number) {
  const transport = createGrpcTransport({ baseUrl: `http://localhost:${port}` })
  if (type === 'system') {
    DynamicPortGrpcClientBase.systemClients.forEach(client => client[symUpdateClientWithPort](transport))
  } else {
    DynamicPortGrpcClientBase.userClients.forEach(client => client[symUpdateClientWithPort](transport))
  }
}

const symProxiedClient = Symbol('proxiedClient')
const symUpdateClientWithPort = Symbol('updateClientWithPort')

class DynamicPortGrpcClientBase<T extends DescService> {
  public [symProxiedClient]: Client<T> | null = null
  
  public static systemClients: DynamicPortGrpcClientBase<any>[] = []
  public static userClients: DynamicPortGrpcClientBase<any>[] = []

  constructor(type: GrpcClientType, private service: T) {
    if (type === 'system') {
      DynamicPortGrpcClientBase.systemClients.push(this)
    } else {
      DynamicPortGrpcClientBase.userClients.push(this)
    }
  }
  
  [symUpdateClientWithPort](transport: Transport) {
    this[symProxiedClient] = createClient(this.service, transport)
  }
}

export const grpcClients = {
  hostInfo: createDynamicPortGrpcClient('system', HostInfoService),
  power: createDynamicPortGrpcClient('system', PowerService),
  desktop: createDynamicPortGrpcClient('user', DesktopService),
}

function createDynamicPortGrpcClient<T extends DescService>(type: GrpcClientType, service: T) {
  const proxyObj = new Proxy(new DynamicPortGrpcClientBase(type, service), {
    get(obj, methodName) {
      if (Object.keys(service.method).includes(methodName as string)) {
        return (obj[symProxiedClient] as any)[methodName].bind(obj)
      } else {
        return (obj as any)[methodName].bind(obj)
      }
    }
  })

  return proxyObj as Client<T> & DynamicPortGrpcClientBase<T>
}

