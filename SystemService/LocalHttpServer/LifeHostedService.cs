using SparkerSystemService.Pipes;

namespace SparkerSystemService.LocalHttpServer;

public class LifeHostedService : IHostedService
{
  public Task StartAsync(CancellationToken cancellationToken)
  {
    _ = PipeSingletons.PipeToServer.WritePortReport(LocalHttpServerService.Port);
    return Task.CompletedTask;
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }
}