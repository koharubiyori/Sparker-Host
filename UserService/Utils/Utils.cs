
using System.Reflection;
using Windows.Win32;
using Serilog;
using SparkerUserService.Preferences;

namespace SparkerUserService.Utils;

public static class Utils
{
  public static Stream ReadResourceAsStream(string name)
  {
    var assembly = Assembly.GetExecutingAssembly();
    var resNamespace = "SparkerUserService.Resources.";

    return assembly.GetManifestResourceStream(resNamespace + name)!;
  }

  public static void WelcomeMessage()
  {
    if (!Preference.Reminder.FirstRun) return;
    TrayIconManager.ShowNotification(Resources.Strings.Welcome);
    Preference.Reminder.FirstRun = false;
  }
  
  // public static unsafe bool IsSessionLocked()
  // {
  //   var hWnd = PInvoke.GetForegroundWindow();
  //   if (hWnd.IsNull) return true;
  //   uint processId;
  //   var result = PInvoke.GetWindowThreadProcessId(hWnd, &processId);
  //   if (result == 0) return true;
  //   
  //   var process = System.Diagnostics.Process.GetProcessById((int)processId);
  //   Log.Information(process.ProcessName);
  //   return process.ProcessName.Equals("LockApp", StringComparison.OrdinalIgnoreCase);
  // }
}