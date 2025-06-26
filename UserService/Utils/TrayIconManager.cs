using System.Diagnostics;
using System.Drawing;
using System.ServiceProcess;
using H.NotifyIcon.Core;
using Serilog;
using SparkerCommons;
using SparkerUserService.Pipes;

namespace SparkerUserService.Utils;

public static class TrayIconManager
{
  private static TrayIconWithContextMenu trayIcon;
  
  public static async Task<bool> TryInitialize()
  {
    var untilCreated = new TaskCompletionSource<bool>();
    await using var iconRes = Utils.ReadResourceAsStream("icon.ico");
    var icon = new Icon(iconRes);
    trayIcon = new TrayIconWithContextMenu
    {
      ToolTip = "Sparker",
      Icon = icon.Handle,
    };

    trayIcon.ContextMenu = new PopupMenu
    {
      Items =
      {
        new PopupMenuItem(Resources.Strings.Github, (_, _) => OpenGithubUrl()),
        new PopupMenuSeparator(),
        new PopupMenuItem(Resources.Strings.Restart, (_, _) => PipeToSystemService.Instance.WriteRestart()),
        new PopupMenuItem(Resources.Strings.Quit, (_, _) => PipeToSystemService.Instance.WriteStop())
      }
    };

    trayIcon.SubscribeToCreated((_, _) => untilCreated.SetResult(true));
    try
    {
      trayIcon.Create();
    }
    catch (InvalidOperationException e)
    {
      Log.Error(e, "Failed to create tray icon");
      untilCreated.SetResult(false);
    }
    
    await untilCreated.Task;
    return true;
  }

  public static void Clean()
  {
    trayIcon.Remove();
    trayIcon.Dispose();
  }

  public static void ShowNotification(string message, string title = "Sparker")
  {
    trayIcon.ShowNotification(title, message, realtime: true);
  }

  private static void OpenGithubUrl()
  {
    Process.Start(new ProcessStartInfo
    {
      FileName = Constants.GithubUrl,
      UseShellExecute = true
    });
  }
}