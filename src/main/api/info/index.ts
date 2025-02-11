import serviceRequest from '~/request/serviceRequest'

export const APPROACH = '__approach__'

const infoApi = {
  approach(port: number) {
    return serviceRequest('info/approach', {
      prefixUrl: `http://127.0.0.1:${port}`,
      headers: { [APPROACH]: APPROACH },   // for marking approach to skip the circular call caused by setActiveServicePort()
      json: { widthMacAddress: false }
    }) as any as Promise<void>
  }
}

export default infoApi