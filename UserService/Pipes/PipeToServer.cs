using System.IO.Pipes;
using Serilog;
using SparkerCommons;
using SparkerCommons.Utils;
using SparkerUserService.LocalServices;

namespace SparkerUserService.Pipes;

public class PipeToServer() : NamedPipeEndpoint<NamedPipeClientStream>(Constants.ServerPipeName, "PipeToServer")
{
  public static PipeToServer Instance { get; } = new();
  
  public async Task WritePortReport(int port)
  {
    Log.Information("[{PipeLabel}] Send portReport!", pipeLabel);
    await Write(Constants.ServerPipeMessageType.In.SubmitPort, "user", port.ToString());
  }
  
  protected override async Task OnConnected()
  {
    if (LocalServer.Port != 0) await WritePortReport(LocalServer.Port);
  }
}