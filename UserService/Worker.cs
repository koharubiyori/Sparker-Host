using SparkerUserService.LocalServices;
using SparkerUserService.Pipes;
using SparkerUserService.Utils;

namespace SparkerUserService;

public class Worker : BackgroundService
{
  private readonly LocalServer _localServer = new();
  
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    TrayIconManager.Initialize();

    await Task.WhenAll(
      PipeToServer.Instance.RunAsync(),
      PipeToSystemService.Instance.RunAsync(),
      _localServer.RunAsync(stoppingToken)
    );
  }

  public override async Task StopAsync(CancellationToken cancellationToken)
  {
    TrayIconManager.Clean();
    await Task.WhenAll(
      PipeToServer.Instance.StopAsync(),
      PipeToSystemService.Instance.StopAsync()
    );
  }
}