using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Security;

namespace SparkerSystemService.Utils;

public static class Utils
{
  public static Win32Exception CreateWin32ExceptionByMethodName(string methodName)
  {
    var win32Message = Marshal.GetPInvokeErrorMessage(Marshal.GetLastPInvokeError());
    return new Win32Exception($"{methodName} failed: {win32Message}");
  }

  public static int IsValidCredential(string username, string domain, string password)
  {
    var result = PInvoke.LogonUser(
      username,
      domain.Length == 0 ? "." : domain,
      password,
      LOGON32_LOGON.LOGON32_LOGON_INTERACTIVE,
      LOGON32_PROVIDER.LOGON32_PROVIDER_DEFAULT,
      out var hToken
    );
    return result == 0 ? Marshal.GetLastWin32Error() : 0;
  }
}