import http from 'http'
import { AddressInfo } from 'net'
import WebSocket, { WebSocketServer } from 'ws'
import { isAuthorized } from '../middlewares/authorization'
import { logger } from '../utils/logger'
import { bindWsEventHandlers } from './bindWsEventHandlers'
import { RealtimeServerMessage } from './realtimeServerMessage'

export class RealtimeServer {
  private static serverPath = '/ws'
  private wss: WebSocket.Server | null = null
  private wsConnections: WebSocket[] = []

  constructor(
    private server: http.Server,
    public label: string = 'RealtimeServer'
  ) {}

  run() {
    if (this.wss !== null) throw new Error('RealtimeServer already started')
    this.wss = new WebSocketServer({ noServer: true })
    this.server.on('upgrade', async (req, socket, head) => {
      const token = req.headers['authorization']

      if (req.url !== RealtimeServer.serverPath) {
        socket.write('HTTP/1.1 404 Not Found\r\n\r\n')
        socket.destroy()
        logger.warn('Client requested a non-WebSocket endpoint.', {
          label: this.label
        })
        return
      }

      if (!(await isAuthorized(token))) {
        socket.write('HTTP/1.1 401 Unauthorized\r\n\r\n')
        socket.destroy()
        return
      }

      // Verification passed, so we can upgrade to websocket connection
      this.wss!.handleUpgrade(req, socket, head, (ws) => {
        bindWsEventHandlers(ws, req)
        this.wsConnections.push(ws)
      })
    })

    this.server.on('listening', () => {
      logger.info(
        `Running on http://localhost:${(this.server.address() as AddressInfo).port}${RealtimeServer.serverPath}`,
        { label: this.label }
      )
    })
  }

  stop() {
    this.wss?.close()
    this.wss = null
  }

  broadcast(message: RealtimeServerMessage) {
    this.wsConnections.forEach((item) => item.send(message.toBinary()))
  }
}

