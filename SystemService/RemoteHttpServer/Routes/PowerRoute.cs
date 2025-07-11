using System.ComponentModel.DataAnnotations;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using SparkerSystemService.RemoteHttpServer.Utils;
using SparkerSystemService.Utils;
using SparkerUserService.Grpc;

namespace SparkerSystemService.RemoteHttpServer.Routes;

public static class PowerRoute
{
  public class ShutdownRequest
  {
    [Required] public bool Force { get; set; }
    [Required] public int Timeout { get; set; }
    [Required] public bool Reboot { get; set; }
    [Required] public bool Hybrid { get; set; }
  }

  public static async Task<ResponseData<ResponseData.Empty>> Shutdown(ShutdownRequest request)
  {
    PowerManager.Shutdown(request.Force, request.Reboot, request.Hybrid);
    if (request is { Force: false, Timeout: > 0 })
    {
      await Task.Delay(request.Timeout);
      PowerManager.Shutdown(true, request.Force, request.Hybrid);
    }

    return ResponseData.Success();
  }

  public class SleepRequest
  {
    [Required] public bool Hibernate { get; set; }
  }

  public static ResponseData<ResponseData.Empty> Sleep(SleepRequest request)
  {
    PowerManager.Sleep(request.Hibernate);
    return ResponseData.Success();
  }

  public static async Task<ResponseData<ResponseData.Empty>> Lock()
  {
    var desktopClient = new DesktopService.DesktopServiceClient(await GrpcClientProvider.ChannelTask);
    await desktopClient.LockSessionAsync(new Empty());
    return ResponseData.Success();
  }
  
  public record UnlockResponse(
    bool Success
  );

  public static async Task<ResponseData<UnlockResponse>> Unlock(HttpContext context)
  {
    var authorization = context.Request.Headers.Authorization.ToString();
    var decodedToken = await Tokener.DecodeAsync(authorization);
    var result = await PowerManager.Unlock(decodedToken.Username, decodedToken.Domain, decodedToken.Password);
    if (!result) return ResponseData.Success(new UnlockResponse(false));
    var success = false; 
    var counter = 0;
    while (true)
    {
      if (counter++ < 10) await Task.Delay(100); 
      else break;
      if (!await PowerManager.GetLocked())
      {
        success = true;
        break;
      }
    }
    
    return ResponseData.Success(new UnlockResponse(success));
  }

  public record IsLockedResponse(
    bool Locked
  );

  public static async Task<ResponseData<IsLockedResponse>> IsLocked()
  {
    var locked = await PowerManager.GetLocked();
    return ResponseData.Success(new IsLockedResponse(locked));
  }
}