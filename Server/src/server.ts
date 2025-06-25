import Router from '@koa/router'
import Koa from 'koa'
import { koaBody } from 'koa-body'
import portfinder from 'portfinder'
import { SERVER_PORT } from './constants'
import { authorization } from './middlewares/authorization'
import { errorHandler } from './middlewares/errorHandler'
import { loggerMiddleware } from './middlewares/logger'
import { protoFieldRemover } from './middlewares/protoFieldRemove'
import { pipeToServices } from './pipes/pipeToServices'
import { RealtimeServer } from './realtimeServer/realtimeServer'
import { deviceRoute, hostInfoRoute, powerRoute } from './routes'
import { DeviceManager } from './utils/deviceManager'
import { updateClientsWithPort } from './utils/grpcClient'
import { initializeLogger, logger } from './utils/logger'

const app = new Koa()
const router = new Router()
export let realtimeServer: RealtimeServer = null as any

app
  .use(errorHandler)
  .use(router.routes())
  .use(router.allowedMethods())

router
  .use(koaBody())
  .use(loggerMiddleware(['/power/pair']))
  .use(authorization(['/device/getPairingCode', '/device/pair', '/hostInfo/approach']))
  .use(protoFieldRemover)

router
  .post('/device/getPairingCode', deviceRoute.getPairingCode)
  .post('/device/pair', deviceRoute.pair)
  .post('/device/unpair', deviceRoute.unpair)
  // .post('/device/update', deviceRoute.update)

  .post('/hostInfo/approach', hostInfoRoute.approach)
  .post('/hostInfo/getBasicInfo', hostInfoRoute.getBasicInfo)

  .post('/power/shutdown', powerRoute.shutdown)
  .post('/power/sleep', powerRoute.sleep)
  .post('/power/unlock', powerRoute.unlock)
  .post('/power/lock', powerRoute.lock)
  .post('/power/isLocked', powerRoute.isLocked)


async function run() {
  await initializeLogger()
  await DeviceManager.loadDevicesFromFile()
  
  const [systemServicePort, userServicePort] = process.argv.slice(2)
  if (systemServicePort && systemServicePort !== '0') updateClientsWithPort('system', Number(systemServicePort))
  if (userServicePort && userServicePort !== '0') updateClientsWithPort('user', Number(userServicePort))
  
  pipeToServices.run()
  const availablePort = await portfinder.getPortPromise({ port: SERVER_PORT })
  const koaServer = app.listen(availablePort, () => {
    logger.info(`Server is running on http://localhost:${availablePort}`)
  })
  realtimeServer = new RealtimeServer(koaServer)
  realtimeServer.run()

  return () => {
    koaServer.close()
    pipeToServices.stop()
    realtimeServer.stop()
  }
}

export let cleanServer: () => void = null as any

run().then(value => cleanServer = value)