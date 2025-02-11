import p from 'path'
import { app, Menu, MenuItem, nativeImage, Tray } from 'electron'
import { SERVICE_NAME } from '~/constants'
import settingsPreference from '~/main/preferences/settings'
import { globalI18n } from '~/utils/i18n'
import serviceManager from '~/main/utils/serviceManager'
import settingsApi from './api/settings'
import { getPackagedResourcePath } from './utils/utils'

export default async function createTray() {
  const i18n = globalI18n()
  const icon = nativeImage.createFromPath(getPackagedResourcePath('assets/tray.ico'))
  const tray = new Tray(icon)

  const launchAtStartupCheckbox = new MenuItem({
    label: i18n.launchAtStartup,
    type: 'checkbox',
    checked: app.getLoginItemSettings().openAtLogin,
    click(menuItem, window, event) {
      app.setLoginItemSettings({ openAtLogin: menuItem.checked })
      serviceManager.config(SERVICE_NAME, { start: menuItem.checked ? 'auto' : 'demand' })
    },
  })

  const lanOnlyCheckbox = new MenuItem({
    label: i18n.lanOnly,
    type: 'checkbox',
    checked: await settingsApi.setLanOnly(),
    click(menuItem) {
      settingsApi.setLanOnly(menuItem.checked)
    }
  })

  const welcomeNotificationCheckbox = new MenuItem({
    label: i18n.showWelcomeNotification,
    type: 'checkbox',
    checked: settingsPreference.showWelcomeNotification,
    click(menuItem) {
      settingsPreference.showWelcomeNotification = menuItem.checked
    }
  })

  const exitButton = new MenuItem({
    label: i18n.exit,
    async click() {
      await serviceManager.stop(SERVICE_NAME)
      app.quit()
    }
  })

  const contextMenu = Menu.buildFromTemplate([
    launchAtStartupCheckbox,
    lanOnlyCheckbox,
    welcomeNotificationCheckbox,
    exitButton
  ])

  tray.on('click', () => tray.popUpContextMenu())

  tray.setContextMenu(contextMenu)
  tray.setTitle(i18n.appName)
  tray.setToolTip(i18n.appName)
  
  return tray
}