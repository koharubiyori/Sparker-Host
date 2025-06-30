import net from 'net'
import { StringArrayPacker } from '../utils/stringArrayPacker'
import { logger } from '../utils/logger'

export abstract class NamedPipeServer {
  abstract messageHandler(messages: string[]): void
  
  private server: net.Server | null = null
  private get pipePath(): string { return `\\\\.\\pipe\\${this.pipeName}` }
  
  constructor(
    public pipeName: string,
    public pipeLabel: string,
  ) {}

  run() {
    this.server = net.createServer((socket) => {
      socket.on('data', (data) => {
        const messages = StringArrayPacker.unpack(data)
        messages.forEach(item => this.messageHandler(item))
      })

      socket.on('error', (error) => {
        logger.error('Pipe connection error', { label: this.pipeLabel, error })
      })
    })

    this.server.listen(this.pipePath, () => {
      logger.info('Started!', { label: this.pipeLabel })
    })

    this.server.on('error', (error) => {
      if ((error as any).code === 'EADDRINUSE') {
        logger.error('Named pipe already in use!', { label: this.pipeLabel })
      } else {
        logger.error('Pipe error', { label: this.pipeLabel, error })
      }
    })
  }

  stop() {
    this.server?.close()
  }
}