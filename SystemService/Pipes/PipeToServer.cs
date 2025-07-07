using System.IO.Pipes;
using Commons;
using Serilog;
using ServiceShared.Utils;
using SparkerSystemService.LocalServices;

namespace SparkerSystemService.Pipes;

public class PipeToServer() : NamedPipeEndpoint<NamedPipeClientStream>(Constants.ServerPipeName, "PipeToServer")
{
  protected override async Task OnConnected()
  {
    if (LocalHttpServer.Port != 0) await WritePortReport(LocalHttpServer.Port);
  }

  public async Task WritePortReport(int port)
  {
    Log.Information("[{PipeLabel}] Send portReport!", pipeLabel);
    await Write(Constants.ServerPipeMessageType.In.SubmitPort, "system", port.ToString());
  }

  public async Task WriteClose()
  {
    Log.Information("[{PipeLabel}] Send close!", pipeLabel);
    await Write(Constants.ServerPipeMessageType.In.Stop);
  }
}