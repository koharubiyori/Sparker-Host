import { PIPE_NAME } from '../constants'
import { RealtimeServerMessage } from '../realtimeServer/realtimeServerMessage'
import { cleanServer, realtimeServer } from '../server'
import { GrpcClientType, updateClientsWithPort } from '../utils/grpcClient'
import { logger } from '../utils/logger'
import { NamedPipeServer } from './namedPipeServer'

enum InMessageType {
  SubmitPort = 'submitPort',
  Stop = 'stop',
  RequestToUnlock = 'requestToUnlock',
}

class PipeToServices extends NamedPipeServer {
  constructor() {
    super(PIPE_NAME, 'PipeToServices')
  }

  async messageHandler([type, ...data]: [InMessageType, ...string[]]) {
    logger.info(`Received message: ${type} [${data.join(', ')}]`, { label: this.pipeLabel })
    if (type === InMessageType.SubmitPort) {
      const service = data[0] as GrpcClientType
      const port = Number(data[1])
      updateClientsWithPort(service, port)
    }
    if (type === InMessageType.Stop) {
      cleanServer()
      process.exit(0)
    }
    if (type === InMessageType.RequestToUnlock) {
      realtimeServer.broadcast(RealtimeServerMessage.createFunctionMessage('requestToUnlockMessage', {}))
    }
  }
}

export const pipeToServices = new PipeToServices()

