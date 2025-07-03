using System.IO.Pipes;
using Commons;
using Serilog;
using ServiceShared.Utils;
using SparkerSystemService.LocalServices;

namespace SparkerSystemService.Pipes;

public class PipeToServer() : NamedPipeEndpoint<NamedPipeClientStream>(Constants.ServerPipeName, "PipeToServer")
{
  public static PipeToServer Instance { get; } = new();

  protected override async Task OnConnected()
  {
    if (LocalServer.Port != 0) await WritePortReport(LocalServer.Port);
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