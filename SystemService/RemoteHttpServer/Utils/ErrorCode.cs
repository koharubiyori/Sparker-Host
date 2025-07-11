namespace SparkerSystemService.RemoteHttpServer.Utils;

public enum ErrorCode
{
  Success = 0,
  ParameterError = 1,
  UnknownError = 99999,
  InternalError = 99998,
  GrpcConnectError = 99997,

  // Device errors
  DeviceUsernameOrPasswordInvalid = 10000,
  DeviceNotPaired = 10001,
  DeviceInvalidPairingCode = 10002
}