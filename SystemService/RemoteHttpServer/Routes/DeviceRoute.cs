using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SparkerSystemService.Preferences;
using SparkerSystemService.RemoteHttpServer.Middlewares;
using SparkerSystemService.RemoteHttpServer.Utils;
using SparkerSystemService.Utils;

namespace SparkerSystemService.RemoteHttpServer.Routes;

public static class DeviceRoute
{
  public class GetPairingCodeRequest
  {
    [Required] public string SessionId { get; set; } = null!;
  }

  public static async Task<ResponseData<ResponseData.Empty>> GetPairingCode(GetPairingCodeRequest request)
  {
    var pairingCode = PairingCodeManager.Get(request.SessionId);
    await TinyUtils.ShowToast(string.Format(Resources.Strings.F_DevicePairingCode, pairingCode),
      Resources.Strings.S_DevicePairingCodeDesc);
    return ResponseData.Success();
  }
  
  public class PairRequest
  {
    [Required] public string DeviceId { get; set; } = null!;
    [Required] public string PairingCode { get; set; } = null!;
    [Required] public string SessionId { get; set; } = null!;
    [Required] public string Username { get; set; } = null!;
    [Required] public string Domain { get; set; } = null!;
    [Required] public string Password { get; set; } = null!;
  }

  public record PairResponse(
    string Token
  );
  
  public static async Task<ResponseData<PairResponse>> Pair(PairRequest request)
  {
    var result = TinyUtils.IsValidCredential(request.Username, request.Domain, request.Password);
    if (result != 0) throw new RemoteHttpServerException<object?>(ErrorCode.DeviceUsernameOrPasswordInvalid, "Invalid username or password");

    if (PairingCodeManager.Validate(request.SessionId, request.PairingCode))
    {
      var token = await Tokener.SignAsync(new DecodedToken(
        DeviceId: request.DeviceId,
        Username: request.Username,
        Domain: request.Domain,
        Password: request.Password
      ));
      Preference.PairedDevice.AddDevice(request.DeviceId, new PairedDevice());
      return ResponseData.Success(new PairResponse(token));
    }
    else
    {
      throw new RemoteHttpServerException<object?>(ErrorCode.DeviceInvalidPairingCode, "Invalid pairing code");
    }
  }
  
  public class UnpairRequest
  {
    [Required] public string DeviceId { get; set; } = null!;
  }
  
  public static ResponseData<ResponseData.Empty> Unpair(UnpairRequest request)
  {
    if (!Preference.PairedDevice.Exists(request.DeviceId)) throw new RemoteHttpServerException<object?>(ErrorCode.DeviceNotPaired, "Device not paired");
    Preference.PairedDevice.RemoveDevice(request.DeviceId);
    return ResponseData.Success();
  }
}