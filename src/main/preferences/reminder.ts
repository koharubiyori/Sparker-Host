import Preference from './utils/preference'

const defaultValue = {
  firstRun: true
}

const preference = new Preference('reminder', defaultValue)
const reminderPreference = preference.value

export default reminderPreference
export const initReminderPreference = () => preference.init()