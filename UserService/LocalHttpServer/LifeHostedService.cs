
using SparkerUserService.Pipes;
using SparkerUserService.Utils;

namespace SparkerUserService.LocalServices;

public class LifeHostedService : IHostedService
{
  public Task StartAsync(CancellationToken cancellationToken)
  {
    _ = PipeSingletons.PipeToSystemService.WriteSubmitPort(LocalHttpServer.LocalHttpServerService.Port);
    return Task.CompletedTask;
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }
}