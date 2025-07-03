using System.ServiceProcess;
using Serilog;
using ServiceShared;
using SparkerSystemService.LocalServices;
using SparkerSystemService.Pipes;
using SparkerSystemService.Utils;

namespace SparkerSystemService;

public class SystemServiceModule : IServiceModule
{
  private static CancellationTokenSource? _stoppingCts;

  private static IHost _host;
  private readonly LocalServer _localServer = new();
  
  
  public SystemServiceModule(IHost host)
  {
    _host = host;
  }
  
  public static void Clean()
  {
    _stoppingCts?.Cancel();
  }

  public static void Stop()
  {
    _host.StopAsync().Wait();
  }

  public static void Initialize()
  {
    LoggerInitializer.Initialize();
  }
  
  public async Task ExecuteAsync(CancellationToken stoppingToken = default)
  {
    Log.Information("Starting SparkerService");
    _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
    await Task.WhenAll(
      PipeToCred.Instance.RunAsync(stoppingToken),
      PipeToServer.Instance.RunAsync(stoppingToken),
      PipeToUserService.Instance.RunAsync(stoppingToken), 
#if !DEBUG
      ChildServiceLauncher.RunAsync(stoppingToken),
#endif
      _localServer.RunAsync(stoppingToken)
    );
  }
  
  public void OnSessionChange(SessionChangeDescription changeDescription)
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
}