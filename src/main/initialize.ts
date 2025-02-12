import { app } from 'electron'
import storage from 'electron-json-storage'
import { PREFERENCE_PATH } from '~/constants'
import reminderPreference, { initReminderPreference } from '~/main/preferences/reminder'
import settingsPreference, { initSettingsPreference } from '~/main/preferences/settings'
import logger from '~/main/utils/logger'
import { isAdministrator, showErrorBox, showNotification } from '~/main/utils/utils'
import { refreshActivePort } from '~/request/serviceRequest'
import { globalI18n } from '~/utils/i18n'

export default async function initializeInMainProgress() {
  const i18n = globalI18n()
  app.setAppUserModelId(i18n.appName)

  try {
    storage.setDataPath(PREFERENCE_PATH)

    await Promise.all([
      initReminderPreference(),
      initSettingsPreference()
    ])

    if (reminderPreference.firstRun) {
      reminderPreference.firstRun = false
    }

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