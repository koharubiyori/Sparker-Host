
using SparkerUserService.Pipes;
using SparkerUserService.Utils;

namespace SparkerUserService.LocalServices;

public class LifeHostedService : IHostedService
{
  public Task StartAsync(CancellationToken cancellationToken)
  {
    _ = PipeToServer.Instance.WritePortReport(LocalServer.Port);
    return Task.CompletedTask;
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }
}