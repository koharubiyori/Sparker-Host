using System.ServiceProcess;
using Microsoft.Extensions.Hosting;

namespace ServiceShared;

public abstract class SessionChangeAwareService : BackgroundService, ISessionChangeHandler
{
  public virtual void OnSessionChange(SessionChangeDescription changeDescription) { }
}