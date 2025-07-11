using System.ComponentModel;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Security;
using Windows.Win32.System.Shutdown;
using Microsoft.Win32;
using Serilog;
using SparkerSystemService.Pipes;

namespace SparkerSystemService.Utils;

public static class PowerManager
{
  private static bool? _locked;

  public static void SetLocked(bool value)
  {
    _locked = value;
  }

  public static async Task<bool> GetLocked()
  {
    _locked ??= await PipeSingletons.PipeToCred.IsCredPipeServerRunning();
    return _locked.Value;
  }

  public static void Shutdown(bool force = false, bool reboot = false, bool hybrid = false)
  {
    EnableShutdownPrivilege();
    var hybridShutdownFlag = hybrid ? EXIT_WINDOWS_FLAGS.EWX_HYBRID_SHUTDOWN : 0;
    var forceFlag = force ? EXIT_WINDOWS_FLAGS.EWX_FORCE : 0;
    var shutdownFlag = reboot ? EXIT_WINDOWS_FLAGS.EWX_REBOOT : EXIT_WINDOWS_FLAGS.EWX_SHUTDOWN | hybridShutdownFlag;
    PInvoke.ExitWindowsEx(
      shutdownFlag | forceFlag,
      SHUTDOWN_REASON.SHTDN_REASON_MAJOR_OTHER | SHUTDOWN_REASON.SHTDN_REASON_FLAG_PLANNED
    );
  }

  public static void Sleep(bool hibernate = false)
  {
    PInvoke.SetSuspendState(hibernate, true, true);
  }

  public static Task<bool> Unlock(string username, string domain, string password)
  {
    return PipeSingletons.PipeToCred.WriteLogonInfo(username, domain, password);
  }
  
  public static bool IsHibernateEnabled()
  {
    const string keyPath = @"SYSTEM\CurrentControlSet\Control\Power";
    const string valueName = "HibernateEnabled";

    using var key = Registry.LocalMachine.OpenSubKey(keyPath);
    if (key == null) return false;
    var value = key.GetValue(valueName);
    return value != null && (int)value == 1;
  }
  
  private static unsafe bool EnableShutdownPrivilege()
  {
    var hToken = HANDLE.Null;
    try
    {
      var openProcessTokenResult = PInvoke.OpenProcessToken(
        PInvoke.GetCurrentProcess(),
        TOKEN_ACCESS_MASK.TOKEN_QUERY | TOKEN_ACCESS_MASK.TOKEN_ADJUST_PRIVILEGES,
        &hToken
      );

      if (!openProcessTokenResult) throw TinyUtils.CreateWin32ExceptionByMethodName("OpenProcessToken");

      LUID luid = new();
      var lookupPrivilegeValueResult = PInvoke.LookupPrivilegeValue("", "SeShutdownPrivilege", out luid);
      if (!lookupPrivilegeValueResult) throw TinyUtils.CreateWin32ExceptionByMethodName("LookupPrivilegeValue");

      TOKEN_PRIVILEGES tp = new()
      {
        PrivilegeCount = 1,
        Privileges = new VariableLengthInlineArray<LUID_AND_ATTRIBUTES>
        {
          [0] = new LUID_AND_ATTRIBUTES { Luid = luid, Attributes = TOKEN_PRIVILEGES_ATTRIBUTES.SE_PRIVILEGE_ENABLED },
        }
      };

      var adjustTokenPrivilegesResult =
        PInvoke.AdjustTokenPrivileges(hToken, false, &tp, (uint)TOKEN_PRIVILEGES.SizeOf(1));
      if (!adjustTokenPrivilegesResult) throw TinyUtils.CreateWin32ExceptionByMethodName("AdjustTokenPrivileges");

      return true;
    }
    catch (Win32Exception ex)
    {
      Log.Error(ex, "Failed to enable shutdown privilege.");
      return false;
    }
    finally
    {
      PInvoke.CloseHandle(hToken);
    }
  }

}