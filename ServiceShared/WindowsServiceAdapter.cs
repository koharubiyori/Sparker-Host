using System.ServiceProcess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ServiceShared;

public interface ISessionChangeHandler
{
  void OnSessionChange(SessionChangeDescription changeDescription);
}

public class WindowsServiceAdapter : ServiceBase
{
  private readonly IHost _host;
  
  public WindowsServiceAdapter(IHost host, string serviceName)
  {
    _host = host;
    ServiceName = serviceName;
    CanStop = true;
    CanHandleSessionChangeEvent = true;
    
    // Make stopping host == stopping service
    var lifetime = _host.Services.GetService<IHostApplicationLifetime>();
    lifetime?.ApplicationStopping.Register(Stop);
  }

  protected override void OnStart(string[] args)
  {
    Task.Run(() => _host.StartAsync());
  }

  protected override void OnStop()
  {
    _host.StopAsync().Wait();
  }

  protected override void OnSessionChange(SessionChangeDescription changeDescription)
  {
    base.OnSessionChange(changeDescription);
    var sessionHandler = _host.Services.GetService<ISessionChangeHandler>();
    sessionHandler?.OnSessionChange(changeDescription);
  }
}

