using System.IO.Pipes;
using SparkerCommons;
using SparkerCommons.Utils;

namespace SparkerSystemService.Pipes;

public class PipeToUserService()
  : NamedPipeEndpoint<NamedPipeServerStream>(Constants.SystemServicePipeName, "PipeToUserService")
{
  public static PipeToUserService Instance { get; } = new();
  
  protected override async Task HandleMessage(string[] message)
  {
    await base.HandleMessage(message);
    var eventType = message[0];

    switch (eventType)
    {
      case Constants.SystemServicePipeEvents.In.Restart:
        SparkerService.Clean();
        Environment.FailFast("Manually triggering the service to restart.");
        break;
      case Constants.SystemServicePipeEvents.In.Stop:
        SparkerService.Stop();
        break;
      
    }
  }
}