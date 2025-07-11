using System.ComponentModel;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Security;
using Windows.Win32.System.JobObjects;
using Windows.Win32.System.Threading;
using Serilog;
using SparkerSystemService.LocalServices;
using Constants = SparkerCommons.Constants;

namespace SparkerSystemService.Utils;

public static class ChildServiceLauncher
{
  private static HANDLE _jobHandle;
  private static HANDLE _userToken;
  private static uint _currentActiveSessionId;
  private static bool _stopFlag;

  public static async Task RunAsync()
  {
    _stopFlag = false;
    await LaunchServicesWithSessionWatcher();
  }

  public static void Stop()
  {
    _stopFlag = true;
    Clean();
  }

  private static void Clean()
  {
    if (!_jobHandle.IsNull) PInvoke.CloseHandle(_jobHandle);
    if (!_userToken.IsNull) PInvoke.CloseHandle(_userToken);
    _jobHandle = HANDLE.Null;
    _userToken = HANDLE.Null;
  }

  private static unsafe void InitializeJobObject()
  {
    try
    {
      _jobHandle = PInvoke.CreateJobObject(null, new PCWSTR(null));
      if (_jobHandle.IsNull) throw Utils.CreateWin32ExceptionByMethodName("CreateJobObject");

      var basicLimits = new JOBOBJECT_BASIC_LIMIT_INFORMATION
      {
        LimitFlags = JOB_OBJECT_LIMIT.JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE
      };
      var extendedInfo = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION
      {
        BasicLimitInformation = basicLimits
      };
      int length = Marshal.SizeOf<JOBOBJECT_EXTENDED_LIMIT_INFORMATION>();
      if (!PInvoke.SetInformationJobObject(
            _jobHandle,
            JOBOBJECTINFOCLASS.JobObjectExtendedLimitInformation,
            &extendedInfo,
            (uint)length)
         )
      {
        throw Utils.CreateWin32ExceptionByMethodName("SetInformationJobObject");
      }
    }
    catch
    {
      if (!_jobHandle.IsNull) PInvoke.CloseHandle(_jobHandle);
      throw;
    }
  }

  private static void InitializeUserToken()
  {
    _userToken = GetElevatedUserToken();
  }

  // Copy a general user token from the current active session.
  // private static unsafe HANDLE GetInteractiveUserToken()
  // {
  //   HANDLE activeSessionToken = HANDLE.Null;
  //   try
  //   {
  //     BOOL result;
  //     var sessionId = PInvoke.WTSGetActiveConsoleSessionId();
  //     if (sessionId == 0xffffff) throw Utils.CreateWin32ExceptionByMethodName("WTSGetActiveConsoleSessionId");
  //
  //     result = PInvoke.WTSQueryUserToken(sessionId, ref activeSessionToken);
  //     if (!result) throw Utils.CreateWin32ExceptionByMethodName("WTSQueryUserToken");
  //
  //     var newToken = HANDLE.Null;
  //     result = PInvoke.DuplicateTokenEx(
  //       activeSessionToken,
  //       TOKEN_ACCESS_MASK.TOKEN_ALL_ACCESS,
  //       null,
  //       SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
  //       TOKEN_TYPE.TokenPrimary,
  //       &newToken
  //     );
  //     if (!result) throw Utils.CreateWin32ExceptionByMethodName("DuplicateTokenEx");
  //     return newToken;
  //   }
  //   finally
  //   {
  //     if (!activeSessionToken.IsNull) PInvoke.CloseHandle(activeSessionToken);
  //   }
  // }

  // Create a token from current process(localSystem) with a user session's id. 
  private static unsafe HANDLE GetElevatedUserToken()
  {
    var activeSessionToken = HANDLE.Null;
    try
    {
      var sessionId = PInvoke.WTSGetActiveConsoleSessionId();
      if (sessionId == 0xffffff) throw Utils.CreateWin32ExceptionByMethodName("WTSGetActiveConsoleSessionId");

      var result = PInvoke.OpenProcessToken(
        PInvoke.GetCurrentProcess(),
        TOKEN_ACCESS_MASK.TOKEN_DUPLICATE,
        &activeSessionToken
      );
      if (!result) throw Utils.CreateWin32ExceptionByMethodName("OpenProcessToken");

      var newToken = HANDLE.Null;
      result = PInvoke.DuplicateTokenEx(
        activeSessionToken,
        TOKEN_ACCESS_MASK.TOKEN_ALL_ACCESS,
        null,
        SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
        TOKEN_TYPE.TokenPrimary,
        &newToken
      );
      if (!result) throw Utils.CreateWin32ExceptionByMethodName("DuplicateTokenEx");

      result = PInvoke.SetTokenInformation(
        newToken,
        TOKEN_INFORMATION_CLASS.TokenSessionId,
        &sessionId,
        sizeof(uint)
      );
      if (!result) throw Utils.CreateWin32ExceptionByMethodName("SetTokenInformation");

      return newToken;
    }
    finally
    {
      if (!activeSessionToken.IsNull) PInvoke.CloseHandle(activeSessionToken);
    }
  }
  
  private static unsafe void LaunchInJob(
    HANDLE userToken,
    string executable,
    string arguments,
    out PROCESS_INFORMATION processInfo
  )
  {
    var lpDesktop = Marshal.StringToHGlobalUni("winsta0\\default");
    var startupInfo = new STARTUPINFOEXW();

    try
    {
      // Initialize startupInfo
      BOOL result;
      startupInfo.StartupInfo.cb = (uint)Marshal.SizeOf<STARTUPINFOEXW>();
      startupInfo.StartupInfo.lpDesktop = new PWSTR(lpDesktop);
      startupInfo.StartupInfo.dwFlags = STARTUPINFOW_FLAGS.STARTF_USESTDHANDLES;

      nuint lpSize = 0;
      Native.InitializeProcThreadAttributeList(IntPtr.Zero, 1, 0, &lpSize);
      if (lpSize == 0) throw Utils.CreateWin32ExceptionByMethodName("InitializeProcThreadAttributeList");
      startupInfo.lpAttributeList = (LPPROC_THREAD_ATTRIBUTE_LIST)Marshal.AllocHGlobal((int)lpSize);
      result = PInvoke.InitializeProcThreadAttributeList(startupInfo.lpAttributeList, 1, 0, &lpSize);
      if (!result) throw Utils.CreateWin32ExceptionByMethodName("InitializeProcThreadAttributeList");

      fixed (HANDLE* pJobHandle = &_jobHandle)
      {
        result = PInvoke.UpdateProcThreadAttribute(
          startupInfo.lpAttributeList,
          0,
          PInvoke.PROC_THREAD_ATTRIBUTE_JOB_LIST,
          pJobHandle,
          (uint)IntPtr.Size
        );
      }

      if (!result) throw Utils.CreateWin32ExceptionByMethodName("UpdateProcThreadAttribute");

      // Create Process
      var cmdChars = arguments.ToCharArray();
      processInfo = new PROCESS_INFORMATION();

      fixed (char* pExePath = executable)
      fixed (char* pCmdLine = cmdChars)
      fixed (PROCESS_INFORMATION* pProcessInfo = &processInfo)
      {
        result = Native.CreateProcessAsUserW(
          userToken,
          pExePath,
          pCmdLine,
          null,
          null,
          bInheritHandles: true,
          dwCreationFlags: PROCESS_CREATION_FLAGS.CREATE_UNICODE_ENVIRONMENT |
                           PROCESS_CREATION_FLAGS.CREATE_NO_WINDOW |
                           PROCESS_CREATION_FLAGS.EXTENDED_STARTUPINFO_PRESENT,
          lpEnvironment: null,
          lpCurrentDirectory: null,
          lpStartupInfo: &startupInfo,
          lpProcessInformation: pProcessInfo
        );
        if (!result) throw Utils.CreateWin32ExceptionByMethodName("CreateProcessAsUserW");
      }
    }
    catch (Win32Exception ex)
    {
      Log.Error(ex, "Failed to launch child service.");
      throw;
    }
    finally
    {
      if (!startupInfo.lpAttributeList.IsNull)
      {
        PInvoke.DeleteProcThreadAttributeList(startupInfo.lpAttributeList);
        Marshal.FreeHGlobal(startupInfo.lpAttributeList);
      }

      Marshal.FreeHGlobal(lpDesktop);
    }
  }

  private static unsafe void LaunchInJobWithAutoRestart(HANDLE userToken, string executable, string arguments = "")
  {
    uint exitCode = 0;
    do
    {
      Log.Information("Child service starting: {Executable}", executable);
      LaunchInJob(userToken, executable, arguments, out var processInfo);
      Log.Information("Child service started: {ExitCode}", executable);
      PInvoke.WaitForSingleObject(processInfo.hProcess, UInt32.MaxValue);
      var ok = PInvoke.GetExitCodeProcess(processInfo.hProcess, &exitCode);
      if (!ok) exitCode = 1;
      Log.Information("Child service exited: {Executable} with ExitCode {ExitCode}", executable, exitCode);
      PInvoke.CloseHandle(processInfo.hProcess);
      PInvoke.CloseHandle(processInfo.hThread);
      if (exitCode != 0 && !_stopFlag) Log.Warning("Child service will be restarted in 3 seconds: {Executable}", executable);
      Task.Delay(3000).Wait();
    } while (exitCode != 0 && !_stopFlag);
  }

  private static async Task LaunchServices()
  {
    await Task.WhenAll(
      Task.Run(() => LaunchInJobWithAutoRestart(_userToken, Constants.ServerExePath, $"{LocalServer.Port}")),
      Task.Run(() => LaunchInJobWithAutoRestart(_userToken, Constants.UserServiceExePath))
    );
  }

  private static async Task LaunchServicesWithSessionWatcher()
  {
    do
    {
      try
      {
        var activeSessionId = PInvoke.WTSGetActiveConsoleSessionId();
        if (activeSessionId == 0xffffff) throw Utils.CreateWin32ExceptionByMethodName("WTSGetActiveConsoleSessionId");
        if (activeSessionId != _currentActiveSessionId)
        {
          Log.Information("The child services restart with a new user session: {sessionId}", activeSessionId);
          Clean();
          InitializeJobObject();
          InitializeUserToken();
          _ = LaunchServices();
          _currentActiveSessionId = activeSessionId;
        }
      }
      catch (Win32Exception ex)
      {
        Log.Error(ex, "Failed to launch child services.");
      }
      finally
      {
        await Task.Delay(3000);
      }
    } while (!_stopFlag);
  }
}