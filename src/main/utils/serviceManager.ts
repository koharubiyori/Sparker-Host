import childProcess from 'child_process'

const serviceManager = {
  getAllServiceNames() {
    return new Promise<string[]>((resolve, reject) => {
      childProcess.exec('sc query state= all', (err, stdout, stderr) => {
        if (err) return reject(err)
        const result = stdout.toString()
          .split("\r\n")
          .filter(item => item.indexOf('SERVICE_NAME') !== -1)
          .map(item => item.replace('SERVICE_NAME: ', ''))
        resolve(result)
      })
    })
  },

  async exists(serviceName: string) {
    return (await serviceManager.getAllServiceNames()).includes(serviceName)
  },

  create({
    name,
    start = 'auto',
    binpath
  }: CreateServiceOptions) {
    return new Promise<void>((resolve, reject) => {
      childProcess.exec(`sc create "${name}" start= ${start} binpath= "${binpath}"`, (err) => err ? reject(err) : resolve())
    })
  },

  start(serviceName: string) {
    return new Promise<void>((resolve, reject) => {
      childProcess.exec(`sc start "${serviceName}"`, (err) => err ? reject(err) : resolve())
    })
  },

  stop(serviceName: string) {
    return new Promise<void>((resolve, reject) => {
      childProcess.exec(`sc stop "${serviceName}"`, (err) => err ? reject(err) : resolve())
    })
  },

  description(serviceName: string, content: string) {
    return new Promise<void>((resolve, reject) => {
      childProcess.exec(`sc description "${serviceName}" "${content}"`, (err) => err ? reject(err) : resolve())
    })
  },

  failure(serviceName: string, reset = 0, actions = 'restart/5000') {
    return new Promise<void>((resolve, reject) => {
      childProcess.exec(`sc failure "${serviceName}" reset= ${reset} actions= ${actions}`, (err) => err ? reject(err) : resolve())
    })
  },

  delete(serviceName: string) {
    return new Promise<void>((resolve, reject) => {
      childProcess.exec(`sc delete "${serviceName}"`, (err) => err ? reject(err) : resolve())
    })
  },

  config(serviceName: string, options: ConfigServiceOptions) {
    return new Promise<void>((resolve, reject) => {
      const start = options.start ? `start= ${options.start}` : ''
      childProcess.exec(`sc config "${serviceName}" ${start}`, (err) => err ? reject(err) : resolve())
    })
  }
}

export default serviceManager

type StartType = 'boot' | 'system' | 'auto' | 'demand' | 'disabled' | 'delayed-auto'

interface CreateServiceOptions {
  name: string
  start?: StartType
  binpath: string
}

interface ConfigServiceOptions {
  start?: StartType
}