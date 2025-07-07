using System.IO.Pipes;
using Commons;
using ServiceShared.Utils;

namespace SparkerSystemService.Pipes;

public class PipeToUserService()
  : NamedPipeEndpoint<NamedPipeServerStream>(Constants.SystemServicePipeName, "PipeToUserService")
{
  protected override async Task HandleMessage(string[] message)
  {
    await base.HandleMessage(message);
    var eventType = message[0];

    switch (eventType)
    {
      case Constants.SystemServicePipeEvents.In.Restart:
        SystemServiceModule.Clean();
        Environment.FailFast("Manually triggering the service to restart.");
        break;
      case Constants.SystemServicePipeEvents.In.Stop:
        SystemServiceModule.Stop();
        break;
      
    }
  }
}