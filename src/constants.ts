import p from 'path'

export const __DEV = process.env.NODE_ENV === 'development'
export const SERVICE_NAME = 'Sparker'
export const SETTINGS_REG_KEY = '\\SOFTWARE\\Sparker\\Settings'
export const ROOT_DIR_PATH = p.resolve()  // Do not use __dirname or app.getAppPath() because they will be to resources/app.asar
export const SERVICE_BIN_PATH = p.resolve(__DEV ? 'Sparker-Service.exe' : 'resources/Sparker-Service.exe')
export const PREFERENCE_PATH = p.resolve('uiPreferences')
export const LOG_PATH = p.resolve('uiLogs')

export const DEFAULT_SERVICE_PORT = 13001