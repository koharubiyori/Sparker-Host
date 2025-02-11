import serviceRequest from '~/request/serviceRequest'

type SettingsApiResult<T> = { value: T }

const settingsApi = {
  setLanOnly(value?: boolean) {
    return serviceRequest('settings/setLanOnly', {
      json: { value },
    })
      .json<SettingsApiResult<boolean>>()
      .then(data => data.value)
  }
}

export default settingsApi