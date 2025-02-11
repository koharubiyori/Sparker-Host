import Preference from './utils/preference'

const defaultValue = {
  showWelcomeNotification: true
}

const preference = new Preference('settings', defaultValue)
const settingsPreference = preference.value

export default settingsPreference
export const initSettingsPreference = () => preference.init()