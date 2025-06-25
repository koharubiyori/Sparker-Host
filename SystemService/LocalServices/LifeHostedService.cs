using SparkerSystemService.Pipes;
using SparkerSystemService.Utils;

namespace SparkerSystemService.LocalServices;

public class LifeHostedService : IHostedService
{
  public Task StartAsync(CancellationToken cancellationToken)
  {
    PipeToServer.Instance.WritePortReport(LocalServer.Port);
    return Task.CompletedTask;
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }
}