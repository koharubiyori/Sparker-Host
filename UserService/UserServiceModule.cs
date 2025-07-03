using System.ServiceProcess;
using Serilog;
using ServiceShared;
using SparkerUserService.LocalServices;
using SparkerUserService.Pipes;
using SparkerUserService.Preferences;
using SparkerUserService.Utils;

namespace SparkerUserService;

public class UserServiceModule : IServiceModule
{
  private readonly LocalServer _localServer = new();

  public static void Initialize()
  {
    LoggerInitializer.Initialize();
    Preference.InitializeAllPreferences();

    Log.Information("IsInUserSession: {inUserSession}, IsInteractive: {interactive}, IsElevated: {elevated}", 
      SessionChecker.IsInUserSession(), 
      SessionChecker.IsInteractive(),
      SessionChecker.IsElevated()
    );
  }
  public async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    if (!SessionChecker.IsWorkstationLocked())
    {
      await TrayIconManager.Initialize();
      Utils.Utils.WelcomeMessage();
    }

    try
    {
      await Task.WhenAll(
        PipeToServer.Instance.RunAsync(stoppingToken),
        PipeToSystemService.Instance.RunAsync(stoppingToken),
        _localServer.RunAsync(stoppingToken)
      );
    }
    finally
    {
      TrayIconManager.Clean();
    }
  }
}