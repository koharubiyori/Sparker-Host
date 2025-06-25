using System;
using System.Runtime.InteropServices;
using CredProvider.NET.Interop2;

namespace SparkerCredProvider
{
  [ComVisible(true)]
  [Guid("FC554755-47EF-46A1-8B77-D133127E14F1")]
  [ClassInterface(ClassInterfaceType.None)]
  [ProgId("Sparker.CredentialProvider")]
  public class CredentialProvider : CredentialProviderBase
  {
    public static CredentialView NotActive;
 
    public CredentialProvider() {}

    protected override CredentialView Initialize(_CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus, uint dwFlags)
    {
      var flags = (CredentialFlag)dwFlags;

      Logger.Write($"cpus: {cpus}; dwFlags: {flags}");

      var isSupported = IsSupportedScenario(cpus);

      if (!isSupported)
      {
        if (NotActive == null) NotActive = new CredentialView(this) { Active = false };
        return NotActive;
      }

      var view = new CredentialView(this) { Active = true };

      view.AddField(
        cpft: _CREDENTIAL_PROVIDER_FIELD_TYPE.CPFT_TILE_IMAGE,
        pszLabel: "Logo",
        state: _CREDENTIAL_PROVIDER_FIELD_STATE.CPFS_DISPLAY_IN_BOTH,
        guidFieldType: Guid.Parse(CredentialView.CPFG_CREDENTIAL_PROVIDER_LOGO)
      );

      view.AddField(
        cpft: _CREDENTIAL_PROVIDER_FIELD_TYPE.CPFT_LARGE_TEXT,
        pszLabel: "Title",
        defaultValue: "Sparker",
        state: _CREDENTIAL_PROVIDER_FIELD_STATE.CPFS_DISPLAY_IN_BOTH
      );

      view.AddField(
        cpft: _CREDENTIAL_PROVIDER_FIELD_TYPE.CPFT_COMMAND_LINK,
        pszLabel: "Help text",
        defaultValue: "Unlock the device with the Sparker mobile application",
        state: _CREDENTIAL_PROVIDER_FIELD_STATE.CPFS_DISPLAY_IN_BOTH
      );
      
      // view.AddField(
      //   cpft: _CREDENTIAL_PROVIDER_FIELD_TYPE.CPFT_COMMAND_LINK,
      //   pszLabel: "Click to Request Remote Unlock",
      //   defaultValue: "Request Remote Unlock",
      //   state: _CREDENTIAL_PROVIDER_FIELD_STATE.CPFS_DISPLAY_IN_BOTH
      // );

      return view;
    }

    private static bool IsSupportedScenario(_CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus)
    {
      switch (cpus)
      {
        case _CREDENTIAL_PROVIDER_USAGE_SCENARIO.CPUS_CREDUI:
        case _CREDENTIAL_PROVIDER_USAGE_SCENARIO.CPUS_UNLOCK_WORKSTATION:
        case _CREDENTIAL_PROVIDER_USAGE_SCENARIO.CPUS_LOGON:
        //case _CREDENTIAL_PROVIDER_USAGE_SCENARIO.CPUS_CHANGE_PASSWORD:
        case _CREDENTIAL_PROVIDER_USAGE_SCENARIO.CPUS_PLAP:
        case _CREDENTIAL_PROVIDER_USAGE_SCENARIO.CPUS_INVALID:
          return true;

        default:
          return false;
      }
    }
  }
}
