using System.IO.Pipes;
using SparkerCommons;
using SparkerCommons.Utils;

namespace SparkerUserService.Pipes;

public class PipeToSystemService()
  : NamedPipeEndpoint<NamedPipeClientStream>(Constants.SystemServicePipeName,  "PipeToSystemService")
{
  public static PipeToSystemService Instance { get; } = new();

  public async Task<bool> WriteStop()
  {
    return await Write(Constants.SystemServicePipeEvents.In.Stop);
  }

  public async Task<bool> WriteRestart()
  {
    return await Write(Constants.SystemServicePipeEvents.In.Restart);
  }
}