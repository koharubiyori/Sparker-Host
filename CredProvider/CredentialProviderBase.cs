using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CredProvider.NET.Interop2;
using SparkerCredProvider.Utils;

namespace SparkerCredProvider
{
  public abstract class CredentialProviderBase : ICredentialProvider
  {
    public struct UserInfo
    {
      public string Username { get; set; }
      public string Password { get; set; }
    }
    public UserInfo? ReceivedUserInfo { get; private set; }

    private static readonly uint CREDENTIAL_PROVIDER_NO_DEFAULT = 0xFFFFFF;
    private ICredentialProviderEvents events;
    private ICredentialProviderCredentialEvents fieldEvents;
    public bool SentUnlockRequest { get; private set; }  // Sending request is allowed only once during the lockscreen session

    protected abstract CredentialView Initialize(_CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus, uint dwFlags);

    private CredentialView view;
    private _CREDENTIAL_PROVIDER_USAGE_SCENARIO usage;

    private ICredentialProviderCredential singleCpc;

    public virtual int SetUsageScenario(_CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus, uint dwFlags)
    {
      view = Initialize(cpus, dwFlags);
      singleCpc = new CredentialProviderCredential(view);
      view.credential = singleCpc;
      usage = cpus;

      if (view.Active)
      {
        return HRESULT.S_OK;
      }
      
      return HRESULT.E_NOTIMPL;
    }

    public virtual int SetSerialization(ref _CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION pcpcs)
    {
      Logger.Write($"ulAuthenticationPackage: {pcpcs.ulAuthenticationPackage}");

      return HRESULT.S_OK;
    }

    public void ReleaseReceivedUserInfo()
    {
      ReceivedUserInfo = null;
      GC.Collect();   // To immediately free up memory containing password
      GC.WaitForPendingFinalizers();
    }
    
    public async Task SendUnlockRequest()
    {
      SentUnlockRequest = true;
      await PipeToSystemService.WriteUnlockRequestToAllClients();
    }

    public virtual int Advise(ICredentialProviderEvents pcpe, ulong upAdviseContext)
    {
      Logger.Write($"upAdviseContext: {upAdviseContext}");

      if (pcpe != null)
      {
        events = pcpe;
        Marshal.AddRef(Marshal.GetIUnknownForObject(pcpe));

        PipeToSystemService.OnUserInfoReceived = (username, password) =>
        {
          ReceivedUserInfo = new UserInfo
          {
            Username = username,
            Password = password
          };

          events.CredentialsChanged(upAdviseContext);
        };

        var unused = PipeToSystemService.RunAsync();
      }

      return HRESULT.S_OK;
    }

    public virtual int UnAdvise()
    {
      Logger.Write();

      if (events != null)
      {
        Marshal.Release(Marshal.GetIUnknownForObject(events));
        events = null;
        PipeToSystemService.Stop();
        view.Provider.ReleaseReceivedUserInfo();
      }

      return HRESULT.S_OK;
    }

    public virtual int GetFieldDescriptorCount(out uint pdwCount)
    {
      Logger.Write();

      pdwCount = (uint)view.DescriptorCount;

      Logger.Write($"Returning field count: {pdwCount}");

      return HRESULT.S_OK;
    }

    public virtual int GetFieldDescriptorAt(uint dwIndex, [Out] IntPtr ppcpfd)
    {
      if (view.GetField((int)dwIndex, ppcpfd))
      {
        return HRESULT.S_OK;
      }

      return HRESULT.E_INVALIDARG;
    }

    public virtual int GetCredentialCount(
        out uint pdwCount,
        out uint pdwDefault,
        out int pbAutoLogonWithDefault
    )
    {
      Logger.Write();

      pdwCount = 1;
      pdwDefault = ReceivedUserInfo == null ? CREDENTIAL_PROVIDER_NO_DEFAULT : 0;
      pbAutoLogonWithDefault = ReceivedUserInfo == null ? 0 : 1;

      return HRESULT.S_OK;
    }

    public virtual int GetCredentialAt(uint dwIndex, out ICredentialProviderCredential ppcpc)
    {
      Logger.Write($"dwIndex: {dwIndex}");

      ppcpc = singleCpc;
      return HRESULT.S_OK;
    }

    public virtual _CREDENTIAL_PROVIDER_USAGE_SCENARIO GetUsage()
    {
      return usage;
    }
  }
}
