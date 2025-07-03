using System.Diagnostics;
using System.Drawing;
using Commons;
using H.NotifyIcon.Core;
using SparkerUserService.Pipes;
using SparkerUserService.Resources;

namespace SparkerUserService.Utils;

public static class TrayIconManager
{
  private static TrayIconWithContextMenu trayIcon;
  
  public static async Task Initialize()
  {
    var untilCreated = new TaskCompletionSource();
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
        new PopupMenuItem(Strings.Github, (_, _) => OpenGithubUrl()),
        new PopupMenuSeparator(),
        new PopupMenuItem(Strings.Restart, (_, _) => { _ = PipeToSystemService.Instance.WriteRestart(); }),
        new PopupMenuItem(Strings.Quit, (_, _) => { _ = PipeToSystemService.Instance.WriteStop(); })
      }
    };

    trayIcon.SubscribeToCreated((_, _) => untilCreated.SetResult());
    trayIcon.Create();
    
    await untilCreated.Task;
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
