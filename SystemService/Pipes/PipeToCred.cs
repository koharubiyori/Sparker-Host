using System.IO.Pipes;
using SparkerCommons;
using Serilog;
using ServiceShared.Utils;

namespace SparkerSystemService.Pipes;

public class PipeToCred() : NamedPipeEndpoint<NamedPipeClientStream>(Constants.CredPipeName, "PipeToCred", false)
{
  private async Task<bool> WriteHeartbeat()
  {
    Log.Information("[{PipeLabel}] Send heartbeat!", pipeLabel);
    return await Write(Constants.CredPipeMessageType.In.Ping);
  }

  public async Task<bool> IsCredPipeServerRunning()
  {
    return await WriteHeartbeat();
  }
  
  public async Task<bool> WriteLogonInfo(string username, string domain, string password)
  {
    Log.Information("[{PipeLabel}] Send logonInfo!", pipeLabel);
    return await Write(Constants.CredPipeMessageType.In.Logon, username, domain, password);
  }
}