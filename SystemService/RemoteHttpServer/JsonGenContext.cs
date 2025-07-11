using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using SparkerSystemService.RemoteHttpServer.Routes;
using SparkerSystemService.RemoteHttpServer.Utils;

namespace SparkerSystemService.RemoteHttpServer;

// [JsonSerializable(typeof(Task<IResult>))]

// Exceptions
[JsonSerializable(typeof(Task<ResponseData<string>>))]
[JsonSerializable(typeof(Task<ResponseData<string[]>>))]
[JsonSerializable(typeof(Task<ResponseData<Dictionary<string, string[]>>>))]
[JsonSerializable(typeof(ResponseData<ResponseData.Empty>))]

// Host Info
[JsonSerializable(typeof(HostInfoRoute.ApproachRequest))]
[JsonSerializable(typeof(Task<ResponseData<HostInfoRoute.ApproachResponse>>))]
[JsonSerializable(typeof(Task<ResponseData<HostInfoRoute.GetBasicInfoResponse>>))]

// Power
[JsonSerializable(typeof(PowerRoute.ShutdownRequest))]
[JsonSerializable(typeof(PowerRoute.SleepRequest))]
[JsonSerializable(typeof(Task<ResponseData<PowerRoute.UnlockResponse>>))]
[JsonSerializable(typeof(Task<ResponseData<PowerRoute.IsLockedResponse>>))]

// Devices
[JsonSerializable(typeof(DeviceRoute.GetPairingCodeRequest))]
[JsonSerializable(typeof(DeviceRoute.PairRequest))]
[JsonSerializable(typeof(DeviceRoute.UnpairRequest))]
[JsonSerializable(typeof(Task<ResponseData<DeviceRoute.PairResponse>>))]

public partial class RemoteHttpServerJsonGenContext : JsonSerializerContext;
