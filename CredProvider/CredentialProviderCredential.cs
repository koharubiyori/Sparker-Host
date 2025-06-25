using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using CredProvider.NET.Interop2;
using SparkerCredProvider.Utils;

namespace SparkerCredProvider
{
  public class CredentialProviderCredential : ICredentialProviderCredential
  {
    private readonly CredentialView view;
    private const string IconResourcePath = "SparkerCredProvider.Resources.icon.png";

    public CredentialProviderCredential(CredentialView view)
    {
      Logger.Write();

      this.view = view;
    }

    public virtual int Advise(ICredentialProviderCredentialEvents pcpce)
    {
      Logger.Write();
      view.events = pcpce;
      Marshal.AddRef(Marshal.GetIUnknownForObject(pcpce));
      
      if (pcpce is ICredentialProviderCredentialEvents2 ev2)
      {
        Logger.Write("pcpce is ICredentialProviderCredentialEvents2");
      }

      return HRESULT.S_OK;
    }

    public virtual int UnAdvise()
    {
      Logger.Write();

      Marshal.Release(Marshal.GetIUnknownForObject(view.events));
      view.events = null;
      return HRESULT.S_OK;
    }

    public virtual int SetSelected(out int pbAutoLogon)
    {
      Logger.Write();

      //Set this to 1 if you would like GetSerialization called immediately on selection
      pbAutoLogon = view.Provider.ReceivedUserInfo == null ? 0 : 1;

      return HRESULT.S_OK;
    }

    public virtual int SetDeselected()
    {
      Logger.Write();

      view.Provider.ReleaseReceivedUserInfo();
      return HRESULT.S_OK;
    }

    public virtual int GetFieldState(
        uint dwFieldID,
        out _CREDENTIAL_PROVIDER_FIELD_STATE pcpfs,
        out _CREDENTIAL_PROVIDER_FIELD_INTERACTIVE_STATE pcpfis
    )
    {
      Logger.Write($"dwFieldID: {dwFieldID}");

      view.GetFieldState((int)dwFieldID, out pcpfs, out pcpfis);

      return HRESULT.S_OK;
    }

    public virtual int GetStringValue(uint dwFieldID, out string ppsz)
    {
      Logger.Write($"dwFieldID: {dwFieldID}");

      ppsz = view.GetValue((int)dwFieldID);

      return HRESULT.S_OK;
    }

    private Bitmap tileIcon;

    public virtual int GetBitmapValue(uint dwFieldID, out IntPtr phbmp)
    {
      Logger.Write($"dwFieldID: {dwFieldID}");

      try
      {
        TryLoadUserIcon();
      }
      catch (Exception ex)
      {
        Logger.Write("Error: " + ex);
      }

      phbmp = tileIcon?.GetHbitmap() ?? IntPtr.Zero;

      return HRESULT.S_OK;
    }

    private void TryLoadUserIcon()
    {
      if (tileIcon == null)
      {
        var assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream(IconResourcePath);

        tileIcon = (Bitmap)Image.FromStream(stream);
      }
    }

    public virtual int GetCheckboxValue(uint dwFieldID, out int pbChecked, out string ppszLabel)
    {
      Logger.Write($"dwFieldID: {dwFieldID}");

      pbChecked = 0;
      ppszLabel = "";

      return HRESULT.E_NOTIMPL;
    }

    public virtual int GetSubmitButtonValue(uint dwFieldID, out uint pdwAdjacentTo)
    {
      Logger.Write($"dwFieldID: {dwFieldID}");

      pdwAdjacentTo = 0;

      return HRESULT.E_NOTIMPL;
    }

    public virtual int GetComboBoxValueCount(uint dwFieldID, out uint pcItems, out uint pdwSelectedItem)
    {
      Logger.Write($"dwFieldID: {dwFieldID}");

      pcItems = 0;
      pdwSelectedItem = 0;

      return HRESULT.E_NOTIMPL;
    }

    public virtual int GetComboBoxValueAt(uint dwFieldID, uint dwItem, out string ppszItem)
    {
      Logger.Write($"dwFieldID: {dwFieldID}; dwItem: {dwItem}");

      ppszItem = "";

      return HRESULT.E_NOTIMPL;
    }

    public virtual int SetStringValue(uint dwFieldID, string psz)
    {
      Logger.Write($"dwFieldID: {dwFieldID}; psz: {psz}");

      view.SetValue((int)dwFieldID, psz);

      return HRESULT.S_OK;
    }

    public virtual int SetCheckboxValue(uint dwFieldID, int bChecked)
    {
      Logger.Write($"dwFieldID: {dwFieldID}; bChecked: {bChecked}");

      return HRESULT.E_NOTIMPL;
    }

    public virtual int SetComboBoxSelectedValue(uint dwFieldID, uint dwSelectedItem)
    {
      Logger.Write($"dwFieldID: {dwFieldID}; dwSelectedItem: {dwSelectedItem}");

      return HRESULT.E_NOTIMPL;
    }

    public virtual int CommandLinkClicked(uint dwFieldID)
    {
      Logger.Write($"dwFieldID: {dwFieldID}");
      // if (dwFieldID == 2)
      // {
      //   if (view.Provider.SentUnlockRequest) return HRESULT.E_NOTIMPL;
      //   view.Provider.SendUnlockRequest();
      //   
      //   view.SetValue((int)dwFieldID, "Sent!");
      //   return HRESULT.S_OK;
      // }

      return HRESULT.E_NOTIMPL;
    }

    public virtual int GetSerialization(
        out _CREDENTIAL_PROVIDER_GET_SERIALIZATION_RESPONSE pcpgsr,
        out _CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION pcpcs,
        out string ppszOptionalStatusText,
        out _CREDENTIAL_PROVIDER_STATUS_ICON pcpsiOptionalStatusIcon
    )
    {
      Logger.Write();

      var usage = this.view.Provider.GetUsage();

      pcpgsr = _CREDENTIAL_PROVIDER_GET_SERIALIZATION_RESPONSE.CPGSR_NO_CREDENTIAL_NOT_FINISHED;
      pcpcs = new _CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION();
      ppszOptionalStatusText = "";
      pcpsiOptionalStatusIcon = _CREDENTIAL_PROVIDER_STATUS_ICON.CPSI_NONE;

      //Serialization can be called before the user has entered any values. Only applies to logon usage scenarios
      if (usage == _CREDENTIAL_PROVIDER_USAGE_SCENARIO.CPUS_LOGON || usage == _CREDENTIAL_PROVIDER_USAGE_SCENARIO.CPUS_UNLOCK_WORKSTATION)
      {
        //Determine the authentication package
        Common.RetrieveNegotiateAuthPackage(out var authPackage);

        //Only credential packing for msv1_0 is supported using this code
        Logger.Write($"Got authentication package: {authPackage}. Only local authenticsation package 0 (msv1_0) is supported.");

        if (view.Provider.ReceivedUserInfo == null)
        {
          Logger.Write("No username or password provided.");
          return HRESULT.S_OK;
        }

        var username = view.Provider.ReceivedUserInfo?.Username;
        var password = view.Provider.ReceivedUserInfo?.Password;

        Logger.Write($"Preparing to serialise credential with password...");
        pcpgsr = _CREDENTIAL_PROVIDER_GET_SERIALIZATION_RESPONSE.CPGSR_RETURN_CREDENTIAL_FINISHED;
        pcpcs = new _CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION();

        var inCredSize = 0;
        var inCredBuffer = Marshal.AllocCoTaskMem(0);

        //This should work fine in Windows 10 that only uses the Logon scenario
        //But it could fail for the workstation unlock scanario on older OS's
        if (!PInvoke.CredPackAuthenticationBuffer(0, username, password, inCredBuffer, ref inCredSize))
        {
          Marshal.FreeCoTaskMem(inCredBuffer);
          inCredBuffer = Marshal.AllocCoTaskMem(inCredSize);

          if (PInvoke.CredPackAuthenticationBuffer(0, username, password, inCredBuffer, ref inCredSize))
          {
            ppszOptionalStatusText = string.Empty;
            pcpsiOptionalStatusIcon = _CREDENTIAL_PROVIDER_STATUS_ICON.CPSI_SUCCESS;

            //Better to move the CLSID to a constant (but currently used in the .reg file)
            pcpcs.clsidCredentialProvider = Guid.Parse("00006d50-0000-0000-b090-00006b0b0000");
            pcpcs.rgbSerialization = inCredBuffer;
            pcpcs.cbSerialization = (uint)inCredSize;
            pcpcs.ulAuthenticationPackage = authPackage;
            
            return HRESULT.S_OK;
          }

          ppszOptionalStatusText = "Failed to pack credentials";
          pcpsiOptionalStatusIcon = _CREDENTIAL_PROVIDER_STATUS_ICON.CPSI_ERROR;
          return HRESULT.E_FAIL;
        }
      }

      return HRESULT.S_OK;
    }

    public virtual int ReportResult(
        int ntsStatus,
        int ntsSubstatus,
        out string ppszOptionalStatusText,
        out _CREDENTIAL_PROVIDER_STATUS_ICON pcpsiOptionalStatusIcon
    )
    {
      Logger.Write($"ntsStatus: {ntsStatus}; ntsSubstatus: {ntsSubstatus}");

      ppszOptionalStatusText = "";
      pcpsiOptionalStatusIcon = _CREDENTIAL_PROVIDER_STATUS_ICON.CPSI_NONE;

      return HRESULT.S_OK;
    }

    //public virtual int GetUserSid(out string sid)
    //{
    //  //Logger.Write();

    //  //sid = null;
    //  //return HRESULT.S_FALSE;

    //  sid = this.sid;

    //  Console.WriteLine($"Returning sid: {sid}");
    //  return HRESULT.S_OK;
    //}
  }
}