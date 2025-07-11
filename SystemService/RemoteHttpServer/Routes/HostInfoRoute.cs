using System.ComponentModel.DataAnnotations;
using SparkerSystemService.RemoteHttpServer.Middlewares;
using SparkerSystemService.RemoteHttpServer.Utils;
using SparkerSystemService.Utils;

namespace SparkerSystemService.RemoteHttpServer.Routes;

public static class HostInfoRoute
{
  public class ApproachRequest
  {
    [Required] public bool WithMacAddress { get; set; }
  }

  public record ApproachResponse(
    bool Reached,
    string? MacAddress
  );

  public static ResponseData<ApproachResponse> Approach(ApproachRequest request)
  {
    var macAddress = request.WithMacAddress ? NetworkUtils.GetEthernetMacAddress() : null;
    var result = new ApproachResponse(true, macAddress);
    return ResponseData.Success(result);
  }

  public record GetBasicInfoResponse(
    string MacAddress,
    bool HibernateEnabled
  );

  public static ResponseData<GetBasicInfoResponse> GetBasicInfo()
  {
    var macAddress = NetworkUtils.GetEthernetMacAddress() ?? "";
    var result = new GetBasicInfoResponse(
      MacAddress: macAddress,
      HibernateEnabled: PowerManager.IsHibernateEnabled()
    );

    return ResponseData.Success(result);
  }
}