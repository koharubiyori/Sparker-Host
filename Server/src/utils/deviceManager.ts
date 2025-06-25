import fsPromises from 'fs/promises'
import p from 'path'
import { PAIRED_DEVICES_FILE, WORKING_DIR } from '../constants'
import { logger } from './logger'

export class DeviceManager {
  private static devices: Device[] = []
  private static filePath = PAIRED_DEVICES_FILE

  static async loadDevicesFromFile() {
    try {
      const fileContent = await fsPromises.readFile(await this.filePath, 'utf-8')
      this.devices = JSON.parse(fileContent)
    } catch (error) {
      await fsPromises.mkdir(p.dirname(await this.filePath), { recursive: true })
      await fsPromises.writeFile(await this.filePath, JSON.stringify([]), 'utf-8')
    }
  }

  static async saveDevicesToFile() {
    try {
      await fsPromises.writeFile(await this.filePath, JSON.stringify(this.devices), 'utf-8')
    } catch (error) {
      logger.error('Error saving devices to file:', error)
      throw error
    }
  }

  static exists(id: string) {
    return this.devices.findIndex(item => item.id === id) !== -1
  }

  static async addDevice(device: Device) {
    this.devices = this.devices.filter(item => item.id !== device.id)
    this.devices.push(device)
    await this.saveDevicesToFile()
  }

  // static async update(id: string, options: UpdateDeviceOptions) {
  //   const device = this.devices.find(item => item.id === id)!
  //   if (options.name && device.name !== options.name) {
  //     device.name = options.name
  //   }
    
  //   await this.saveDevicesToFile()
  // }

  static async deleteDevice(id: string) {
    this.devices = this.devices.filter(item => item.id !== id)
    await this.saveDevicesToFile()
  }
}

export interface UpdateDeviceOptions {
  name?: string
}