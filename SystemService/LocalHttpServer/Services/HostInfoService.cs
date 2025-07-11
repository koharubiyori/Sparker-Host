using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using SparkerSystemService.Grpc;
using SparkerSystemService.Utils;

namespace SparkerSystemService.LocalHttpServer.Services;

public class HostInfoService : Grpc.HostInfoService.HostInfoServiceBase
{
  public override Task<ApproachResponse> Approach(ApproachRequest request, ServerCallContext context)
  {
    var macAddress = request.WithMacAddress ? NetworkUtils.GetEthernetMacAddress() : null;
    var result = new ApproachResponse { Reached = true };
    if (macAddress != null) result.MacAddress = macAddress;
    return Task.FromResult(result);
  }

  public override Task<GetBasicInfoResponse> GetBasicInfo(Empty request, ServerCallContext context)
  {
    var macAddress = NetworkUtils.GetEthernetMacAddress() ?? "";
    return Task.FromResult(new GetBasicInfoResponse
    {
      MacAddress = macAddress, 
      HibernateEnabled = PowerManager.IsHibernateEnabled()
    });
  }
  
  public override Task<IsValidCredentialResponse> IsValidCredential(IsValidCredentialRequest request, ServerCallContext context)
  {
    var result = Utils.TinyUtils.IsValidCredential(request.Username, request.Domain, request.Password);
    return Task.FromResult(new IsValidCredentialResponse { Result = result });
  }
}
