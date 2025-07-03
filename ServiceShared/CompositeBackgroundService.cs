using System.ServiceProcess;
using Microsoft.Extensions.Hosting;

namespace ServiceShared;

public interface IServiceModule : ISessionChangeHandler
{
  Task ExecuteAsync(CancellationToken stoppingToken = default);
  void ISessionChangeHandler.OnSessionChange(SessionChangeDescription changeDescription) { }
}

public class CompositeBackgroundService : BackgroundService, ISessionChangeHandler
{
  private readonly IEnumerable<IServiceModule> _modules;
  private readonly List<Task> _runningTasks = [];

  public CompositeBackgroundService(IEnumerable<IServiceModule> modules)
  {
    _modules = modules;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    foreach (var module in _modules) _runningTasks.Add(module.ExecuteAsync(stoppingToken));

    var completed = await Task.WhenAny(_runningTasks);
    if (completed.IsFaulted)
    {
      throw completed.Exception!;
    }
  }

  // public override async Task StopAsync(CancellationToken cancellationToken)
  // {
  //   await Task.WhenAll(_runningTasks);
  //   await base.StopAsync(cancellationToken);
  // }

  public void OnSessionChange(SessionChangeDescription changeDescription)
  {
    foreach (var module in _modules) module.OnSessionChange(changeDescription);
  }
}