using System.ServiceProcess;
using Serilog;
using SparkerSystemService.LocalServices;
using SparkerSystemService.Pipes;
using SparkerSystemService.Utils;

namespace SparkerSystemService;

public class SparkerService : ServiceBase
{
  private LocalServer _localServer;

  public static Action Clean { get; private set; }
  public static Action Stop { get; private set; }

  public SparkerService(LocalServer localServer)
  {
    ServiceName = "SunshineService";
    CanStop = true;
    CanHandleSessionChangeEvent = true;
    _localServer = localServer;

    Clean = _Clean;
    Stop = _Stop;
  }

  private void _Clean()
  {
    ChildServiceLauncher.Stop();
    Task.WaitAll(
      PipeToCred.Instance.StopAsync(),
      PipeToServer.Instance.StopAsync(),
      PipeToUserService.Instance.StopAsync(),
      _localServer.StopAsync()
    );
  }

  private void _Stop()
  {
    base.Stop();
  }

  protected override void OnStart(string[] args)
  {
    Log.Information("Starting SparkerService");
    PipeToCred.Instance.RunAsync();
    PipeToServer.Instance.RunAsync();
    PipeToUserService.Instance.RunAsync();
    ChildServiceLauncher.RunAsync();
    _localServer.RunAsync();
  }

  protected override void OnSessionChange(SessionChangeDescription changeDescription)
  {
    switch (changeDescription.Reason)
    {
      case SessionChangeReason.SessionLock:
        Log.Information("Session lock!");
        PowerManager.SetLocked(true);
        break;

      case SessionChangeReason.SessionUnlock:
        Log.Information("Session unlock!");
        PowerManager.SetLocked(false);
        break;
    }
  }

  protected override void OnStop()
  {
    _Clean();
  }
}