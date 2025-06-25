import Registry from 'winreg'

// export function getInstallLocation() {
//   const regKey = new Registry({
//     hive: Registry.HKLM,
//     key: SETTINGS_REG_KEY,
//   })
  
//   return new Promise<string>((resolve, reject) => {
//     regKey.get(REG_PROPERTIES.installLocation, (e, result) => e ? reject(e) : resolve(result.value))
//   })
// }