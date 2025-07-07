using System.Diagnostics.CodeAnalysis;
using System.ServiceProcess;
using Serilog;
using ServiceShared;
using ServiceShared.Utils;
using SparkerSystemService.LocalHttpServer;
using SparkerSystemService.Pipes;
using SparkerSystemService.Utils;

namespace SparkerSystemService;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddSparkerSystemService(this IServiceCollection collection)
  {
    return collection
      .AddOpenedHostedService<PipeToCred>()
      .AddOpenedHostedService<PipeToServer>()
      .AddOpenedHostedService<PipeToUserService>()
      .AddHostedService<LocalHttpServerService>()
      .AddHostedService<ChildServiceLauncher>()
      .AddHostedService<SystemServiceModule>()
    ;
  }
}

public class SystemServiceModule : SessionChangeAwareService
{
  private static CancellationTokenSource? _stoppingCts;

  private static IHost _host;
  
  public SystemServiceModule(IHost host)
  {
    _host = host;
    PipeSingletons.PipeToCred = _host.Services.GetRequiredService<PipeToCred>();
    PipeSingletons.PipeToServer = _host.Services.GetRequiredService<PipeToServer>();
    PipeSingletons.PipeToUserService = _host.Services.GetRequiredService<PipeToUserService>();
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
    var logger = LoggerInitializer.CreateLoggerConfiguration("system");
    LoggerInitializer.InitializeGlobalLogger(logger);
  }

  protected override Task ExecuteAsync(CancellationToken stoppingToken)
  {
     Log.Information("Starting SparkerService");
     return Task.CompletedTask;
  }
  
  public override void OnSessionChange(SessionChangeDescription changeDescription)
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