using System.IO.Pipes;
using SparkerCommons;
using Serilog;
using ServiceShared.Utils;
using SparkerUserService.LocalServices;

namespace SparkerUserService.Pipes;

public class PipeToServer() : NamedPipeEndpoint<NamedPipeClientStream>(Constants.ServerPipeName, "PipeToServer")
{
  public async Task WriteSubmitPort(int port)
  {
    Log.Information("[{PipeLabel}] Send portReport!", pipeLabel);
    await Write(Constants.ServerPipeMessageType.In.SubmitPort, "user", port.ToString());
  }
  
  protected override async Task OnConnected()
  {
    if (LocalHttpServer.LocalHttpServerService.Port != 0) await WriteSubmitPort(LocalHttpServer.LocalHttpServerService.Port);
  }
}