using Grpc.Core;
using Grpc.Net.Client;
using SparkerUserService.Grpc;

namespace SparkerSystemService.Utils;

public static class GrpcClientProvider
{
  private static readonly TaskCompletionSource<GrpcChannel> _channelSource = new();
  public static Task<GrpcChannel> ChannelTask { get; } = _channelSource.Task;
  public static Task<DesktopService.DesktopServiceClient> Desktop { get; } = ChannelTask.ContinueWith(
    t => new DesktopService.DesktopServiceClient(t.Result),
    TaskContinuationOptions.OnlyOnRanToCompletion
  );

  public static void InitializeChannel(int port)
  {
    var channel = GrpcChannel.ForAddress($"http://localhost:{port}");
    _channelSource.SetResult(channel);
  }
}