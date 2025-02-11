import storage from 'electron-json-storage'
import cloneDeep from 'lodash/cloneDeep'

class Preference<T> {
  private defaultValue: T
  value: T
  
  constructor(
    private storageKey: string,
    defaultValue: T
  ) {
    this.defaultValue = cloneDeep(defaultValue)
    this.value = new Proxy(this.defaultValue as any, {
      get(target: any, getter: string) {
        return target[getter]
      },

      set(target: any, key: string, value: any) {
        target[key] = value
        storage.set(storageKey, target, e => {
          if (e) throw e
        })
        return true
      }
    }) as any
  }

  async init() {
    const currentData = await new Promise((resolve, reject) => storage.get(this.storageKey, (e, data) => e ? reject(e) : resolve(data))) 
    if (currentData === null) { return }

    for (let [key, value] of Object.entries(currentData)) {
      ;(this.defaultValue as any)[key] = value
    }
  }
}

export default Preference