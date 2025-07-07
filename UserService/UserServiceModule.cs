using System.ServiceProcess;
using Serilog;
using ServiceShared;
using SparkerUserService.LocalServices;
using SparkerUserService.Pipes;
using SparkerUserService.Preferences;
using SparkerUserService.Utils;
using ServiceShared.Utils;

namespace SparkerUserService;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddSparkerUserService(this IServiceCollection collection)
  {
    return collection
        .AddOpenedHostedService<PipeToServer>()
        .AddOpenedHostedService<PipeToSystemService>()
        .AddHostedService<LocalHttpServer.LocalHttpServerService>()
        .AddHostedService<UserServiceModule>()
      ;
  }
}

public class UserServiceModule : BackgroundService
{

  public static void Initialize()
  {
    var logger = LoggerInitializer.CreateLoggerConfiguration("user");
    LoggerInitializer.InitializeGlobalLogger(logger);
    Preference.InitializeAllPreferences();

    Log.Information("IsInUserSession: {inUserSession}, IsInteractive: {interactive}, IsElevated: {elevated}", 
      SessionChecker.IsInUserSession(), 
      SessionChecker.IsInteractive(),
      SessionChecker.IsElevated()
    );
  }

  public UserServiceModule(PipeToServer pipeToServer, PipeToSystemService pipeToSystemService)
  {
    PipeSingletons.PipeToServer = pipeToServer;
    PipeSingletons.PipeToSystemService = pipeToSystemService;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    if (!SessionChecker.IsWorkstationLocked())
    {
      await TrayIconManager.Initialize();
      Utils.Utils.WelcomeMessage();
    }

    try
    {
      await Task.Delay(Timeout.Infinite, stoppingToken);
    }
    catch (TaskCanceledException)
    {
      TrayIconManager.Clean();
    }
  }
}