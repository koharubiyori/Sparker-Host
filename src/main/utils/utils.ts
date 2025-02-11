import childProcess from 'child_process'
import { app, dialog, Notification } from 'electron'
import p from 'path'
import { globalI18n } from '~/utils/i18n'

export function isAdministrator() {
  return new Promise<boolean>((resolve) => {
    childProcess.exec('net session', err => resolve(!err))
  })
}

export function showNotification(options: ShowNotificationOptions) {
  const notification = new Notification({
    title: options.title,
    body: options.body,
  })

  if (options.onClick) {
    notification.addListener('click', options.onClick)
  }

  notification.show()
}

export function showErrorBox(content: string) {
  const i18n = globalI18n()
  return dialog.showErrorBox(i18n.appName, content)
}

export interface ShowNotificationOptions {
  title: string
  body: string,
  onClick?(): void
}

export function getPackagedResourcePath(path: string) {
  return app.isPackaged ? p.join(__dirname, path) : path
}