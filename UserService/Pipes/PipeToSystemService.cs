using System.IO.Pipes;
using Serilog;
using SparkerCommons;
using ServiceShared.Utils;

namespace SparkerUserService.Pipes;

public class PipeToSystemService()
  : NamedPipeEndpoint<NamedPipeClientStream>(Constants.SystemServicePipeName,  "PipeToSystemService")
{
  public static PipeToSystemService Instance { get; } = new();

  public async Task<bool> WriteStop()
  {
    Log.Information("[{PipeLabel}] Send stop!", pipeLabel);
    return await Write(Constants.SystemServicePipeEvents.In.Stop);
  }

  public async Task<bool> WriteRestart()
  {
    Log.Information("[{PipeLabel}] Send restart!", pipeLabel);
    return await Write(Constants.SystemServicePipeEvents.In.Restart);
  }

  public async Task<bool> WriteSubmitPort(int port)
  {
    Log.Information("[{PipeLabel}] Send submitPort!", pipeLabel);
    return await Write(Constants.SystemServicePipeEvents.In.SubmitPort, port.ToString());
  }
}