import type http from 'http'
import type WebSocket from 'ws'
import { logger } from '../utils/logger'

export function bindWsEventHandlers(ws: WebSocket, request: http.IncomingMessage) {
  const ip = request.socket.remoteAddress ?? 'Unknown IP'
  const label = ['WebSocket', ip]
  logger.info('Connection opened!', { label })
  ws
    .on('message', (message) => {
      logger.info(`Received message: ${message}`, { label })
    })
    .on('close', () => {
      logger.info('Connection closed!', { label })
    })
    .on('error', (error) => {
      logger.error(`WebSocket error!`, { label, error })
    })
}