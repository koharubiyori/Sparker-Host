import crypto from 'crypto'
import fsPromise from 'fs/promises'
import jwt from 'jsonwebtoken'
import { TOKEN_KEY_FILE } from '../constants'

export class Tokener {
  private static key: Buffer | null = null

  private static async loadKey() {
    if (this.key) return this.key
    try {
      const keyBase64 = await fsPromise.readFile(TOKEN_KEY_FILE, { encoding: 'utf-8' })
      this.key = Buffer.from(keyBase64, 'base64')
    } catch(e) {
      this.key = crypto.randomBytes(32)
      await fsPromise.writeFile(TOKEN_KEY_FILE, this.key.toString('base64'), { encoding: 'utf-8' })
    }

    return this.key
  }

  static async sign(data: DecodedToken) {
    return jwt.sign(data, await this.loadKey(), { algorithm: 'HS256' })
  }

  static async decode(token: string) {
    return (await jwt.verify(token, await this.loadKey(), { algorithms: ['HS256'] })) as DecodedToken
  }
}

export interface DecodedToken {
  deviceId: string
  username: string
  password: string
}