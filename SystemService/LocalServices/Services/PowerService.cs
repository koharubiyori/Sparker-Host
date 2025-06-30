using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using SparkerSystemService.Grpc;
using SparkerSystemService.Utils;

namespace SparkerSystemService.LocalServices.Services;

public class PowerService : Grpc.PowerService.PowerServiceBase
{
  public override async Task<Empty> Shutdown(ShutdownRequest request, ServerCallContext context)
  {
    PowerManager.Shutdown(request.Force, request.Reboot, request.Hybrid);
    if (request is { Force: false, Timeout: > 0 })
    {
      await Task.Delay(request.Timeout);
      PowerManager.Shutdown(true, request.Force, request.Hybrid);
    }
    return new Empty();
  }

  public override Task<Empty> Sleep(SleepRequest request, ServerCallContext context)
  {
    PowerManager.Sleep(request.Hibernate);
    return Task.FromResult(new Empty());
  }

  public override async Task<UnlockResponse> Unlock(UnlockRequest request, ServerCallContext context)
  {
    var result = await PowerManager.Unlock(request.Username, request.Domain, request.Password);
    if (!result) return new UnlockResponse { Success = false };
    var success = await Task.Run(async () =>
    {
      var counter = 0;
      while (true)
      {
        if (counter++ < 10) await Task.Delay(100); 
        else return false;
        if (!await PowerManager.GetLocked()) return true;
      }
    });
    return new UnlockResponse { Success = success };
  }
  
  public override async Task<IsLockedResponse> IsLocked(Empty request, ServerCallContext context)
  {
    return new IsLockedResponse { Locked = await PowerManager.GetLocked() };
  }
}
