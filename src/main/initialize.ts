import { app, dialog } from 'electron'
import storage from 'electron-json-storage'
import Registry from 'winreg'
import { PREFERENCE_PATH, ROOT_DIR_PATH, SERVICE_BIN_PATH, SERVICE_NAME, SETTINGS_REG_KEY } from '~/constants'
import reminderPreference, { initReminderPreference } from '~/main/preferences/reminder'
import settingsPreference, { initSettingsPreference } from '~/main/preferences/settings'
import { refreshActivePort } from '~/request/serviceRequest'
import { globalI18n } from '~/utils/i18n'
import logger from '~/main/utils/logger'
import serviceManager from '~/main/utils/serviceManager'
import { isAdministrator, showErrorBox, showNotification } from '~/main/utils/utils'
import globals from './utils/globals'

export default async function initializeInMainProgress() {
  const i18n = globalI18n()
  app.setAppUserModelId(i18n.appName)

  throw new Error('test')
  try {
    if ((await isAdministrator()) === false) throw new Error('Unable to run without administrator rights.')
    storage.setDataPath(PREFERENCE_PATH)

    await Promise.all([
      initReminderPreference(),
      initSettingsPreference()
    ])

    if (reminderPreference.firstRun) {
      app.setLoginItemSettings({ openAtLogin: true })
      reminderPreference.firstRun = false
    }

    await setInstallLocationInRegedit()
    await createService()
    refreshActivePort()
    if (settingsPreference.showWelcomeNotification) {
      showNotification({
        title: i18n.sparkerIsRunning,
        body: i18n.s_operationHint,
      })
    }
  } catch(err) {
    logger.error(err)
    showErrorBox(i18n.s_serviceFatalError)
    app.quit()
  }
}

const regValueName = 'InstallLocation'

async function setInstallLocationInRegedit() {
  const regKey = new Registry({
    hive: Registry.HKLM,
    key: SETTINGS_REG_KEY,
  })
  
  try {
    await new Promise<void>((resolve, reject) => regKey.create(e => e ? reject(e) : resolve()))
    await new Promise<void>((resolve, reject) => {
      regKey.set(regValueName, Registry.REG_SZ, ROOT_DIR_PATH, (e) => e ? reject(e) : resolve())
    })
  } catch(e) {
    logger.error(e)
    throw new Error('failed to set the install location in Regedit')
  }
}

async function createService() {
  const i18n = globalI18n()
  if (await serviceManager.exists(SERVICE_NAME)) { 
    await serviceManager.start(SERVICE_NAME).catch((e) => logger.warn(e))
    return
  }

  await serviceManager.create({
    name: SERVICE_NAME,
    binpath: SERVICE_BIN_PATH
  })
  await serviceManager.description(SERVICE_NAME, i18n.s_serviceDescription)
  await serviceManager.failure(SERVICE_NAME)
  await serviceManager.start(SERVICE_NAME)
}