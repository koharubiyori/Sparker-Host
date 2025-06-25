import p from 'path'

export const __DEV = process.env.NODE_ENV !== 'production'
export const __OUTPUT_ALL_LOGS = __DEV
// export const __OUTPUT_ALL_LOGS = true
export const __HTTP_SERVER_LOG = true
// export const __HTTP_SERVER_LOG = false
export const SERVER_PORT = 13001

export const PIPE_NAME = '38A2CC49-97C8-49E5-BEF9-93536F9A9D15'
export const SETTINGS_REG_KEY = '\\SOFTWARE\\Sparker\\Settings'

export const WORKING_DIR = __DEV ?process.cwd() : p.dirname(process.execPath)
export const LOG_PATH = p.resolve(WORKING_DIR, './Logs')
export const CONFIG_PATH = p.resolve(WORKING_DIR, './config')
export const PAIRED_DEVICES_FILE = p.resolve(CONFIG_PATH, './devices.json')
export const TOKEN_KEY_FILE = p.resolve(LOG_PATH, './token.key')