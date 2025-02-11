// This can create a promise opening resolve and reject
export default class Promiser<T = any> {
  promise: Promise<T>
  status: 'pending' | 'fulfilled' | 'rejected' = 'pending'
  resolve: (value?: T) => void = () => {}
  reject: (reason?: any) => void = () => {}

  constructor() {
    this.promise = new Promise<T>((resolve, reject) => {
      this.resolve = value => {
        resolve(value!)
        this.status = 'fulfilled'
      }

      this.reject = value => {
        reject(value)
        this.status = 'rejected'
      }
    })
  }
}
