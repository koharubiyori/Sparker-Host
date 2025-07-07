using System.IO.Pipes;
using Commons;
using Serilog;
using ServiceShared.Utils;
using SparkerUserService.LocalServices;

namespace SparkerUserService.Pipes;

public class PipeToServer() : NamedPipeEndpoint<NamedPipeClientStream>(Constants.ServerPipeName, "PipeToServer")
{
  public async Task WritePortReport(int port)
  {
    Log.Information("[{PipeLabel}] Send portReport!", pipeLabel);
    await Write(Constants.ServerPipeMessageType.In.SubmitPort, "user", port.ToString());
  }
  
  protected override async Task OnConnected()
  {
    if (LocalHttpServer.Port != 0) await WritePortReport(LocalHttpServer.Port);
  }
}