using System.Net;
using System.Net.Sockets;
using Commons;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using SparkerSystemService.LocalServices.Services;

namespace SparkerSystemService.LocalServices;

public class LocalServer
{
  private readonly WebApplication _app;
  private TcpListener _tcpListener; // Hold the reference to prevent from GC
  public static int Port { get; private set; }

  public LocalServer()
  {
    var builder = WebApplication.CreateSlimBuilder();
    builder.Services
      .AddSerilog()
      .AddHostedService<LifeHostedService>();
    builder.Services.AddGrpc(); 
    builder.WebHost.ConfigureKestrel(options =>
    {
      options.ListenHandle(CreateTcpListenerHandle(), listenOptions =>
      {
        listenOptions.Protocols = HttpProtocols.Http2;
      });
    });
    
    _app = builder.Build();
    _app.MapGrpcService<HostInfoService>();
    _app.MapGrpcService<PowerService>();
  }
  
  private ulong CreateTcpListenerHandle()
  {
    _tcpListener = new TcpListener(IPAddress.Loopback, Constants.Debug ? Constants.TestSystemServicePort : 0);
    _tcpListener.Start();
    Port = ((IPEndPoint)_tcpListener.LocalEndpoint).Port;
    return (ulong)_tcpListener.Server.Handle.ToInt64();
  }

  public async Task RunAsync(CancellationToken stoppingToken = default)
  {
    try
    {
      await _app.RunAsync(stoppingToken);
    }
    finally
    {
      await StopAsync();
    }
  }

  private async Task StopAsync()
  {
    _tcpListener.Stop();
    await _app.StopAsync();
  }
}