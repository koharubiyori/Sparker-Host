using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace SparkerUserService.Utils;

public static class SessionChecker
{
  public static bool IsInteractive()
  {
    ProcessIdToSessionId((uint)Environment.ProcessId, out var currentSessionId);
    return currentSessionId == WTSGetActiveConsoleSessionId();
  }

  public static bool IsInUserSession()
  {
    if (!ProcessIdToSessionId((uint)Process.GetCurrentProcess().Id, out var sessionId))
      return false;

    if (!WTSQuerySessionInformation(IntPtr.Zero, (int)sessionId, WTS_INFO_CLASS.WTSConnectState, out var buffer,
          out _)) return false;
    int state = Marshal.ReadInt32(buffer);
    WTSFreeMemory(buffer);
    return state == (int)WTS_CONNECTSTATE_CLASS.WTSActive;

  }
  
  public static bool IsElevated()
  {
    using var id = WindowsIdentity.GetCurrent();
    var principal = new WindowsPrincipal(id);
    return principal.IsInRole(WindowsBuiltInRole.Administrator);
  }
  
  public static bool IsWorkstationLocked()
  {
    const int UOI_NAME = 2;
    IntPtr hDesktop = OpenInputDesktop(0, false, 0x100); // READ_CONTROL permission

    if (hDesktop == IntPtr.Zero)
      return false;

    try
    {
      var name = new byte[256];
      int needed = 0;
      bool result = GetUserObjectInformation(hDesktop, UOI_NAME, name, name.Length, ref needed);

      if (!result)
        return false;

      string desktopName = System.Text.Encoding.ASCII.GetString(name).TrimEnd('\0');
      
      return !desktopName.Equals("Default", StringComparison.OrdinalIgnoreCase);
    }
    finally
    {
      CloseDesktop(hDesktop);
    }
  }
  
  [DllImport("kernel32.dll", SetLastError = true)]
  static extern bool ProcessIdToSessionId(uint dwProcessId, out uint pSessionId);

  [DllImport("Wtsapi32.dll", SetLastError = true)]
  static extern bool WTSQuerySessionInformation(
    IntPtr hServer,
    int sessionId,
    WTS_INFO_CLASS wtsInfoClass,
    out IntPtr ppBuffer,
    out int pBytesReturned);

  [DllImport("Wtsapi32.dll")]
  static extern void WTSFreeMemory(IntPtr pointer);

  enum WTS_INFO_CLASS
  {
    WTSConnectState = 14
  }

  enum WTS_CONNECTSTATE_CLASS
  {
    WTSActive = 0,
    WTSConnected = 1,
    WTSConnectQuery = 2,
    WTSShadow = 3,
    WTSDisconnected = 4,
    WTSIdle = 5,
    WTSListen = 6,
    WTSReset = 7,
    WTSDown = 8,
    WTSInit = 9
  }
  
  [DllImport("kernel32.dll")]
  static extern uint WTSGetActiveConsoleSessionId();
  
  [DllImport("user32.dll", SetLastError = true)]
  private static extern IntPtr OpenInputDesktop(uint dwFlags, bool fInherit, uint dwDesiredAccess);

  [DllImport("user32.dll", SetLastError = true)]
  private static extern bool CloseDesktop(IntPtr hDesktop);

  [DllImport("user32.dll", SetLastError = true)]
  private static extern bool GetUserObjectInformation(
    IntPtr hObj,
    int nIndex,
    byte[] pvInfo,
    int nLength,
    ref int lpnLengthNeeded
  );
}