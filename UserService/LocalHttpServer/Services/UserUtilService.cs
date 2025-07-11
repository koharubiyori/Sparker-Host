using Windows.Win32;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using SparkerUserService.Grpc;
using SparkerUserService.Utils;

namespace SparkerUserService.LocalHttpServer.Services;

public class UserUtilService : DesktopService.DesktopServiceBase
{
  public override Task<Empty> ShowToast(ShowToastRequest request, ServerCallContext context)
  {
    TrayIconManager.ShowNotification(request.Content, request.Title);
    return Task.FromResult(new Empty());
  }

  public override Task<Empty> LockSession(Empty request, ServerCallContext context)
  {
    PInvoke.LockWorkStation();
    return Task.FromResult(new Empty());
  }
}