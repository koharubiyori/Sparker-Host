using System.Net;
using System.Net.Sockets;
using Commons;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using ServiceShared.Utils;
using SparkerUserService.LocalServices.Services;

namespace SparkerUserService.LocalServices;

public class LocalHttpServer : BackgroundService
{
  private readonly WebApplication _app;
  private TcpListener _tcpListener; // Hold the reference to prevent from GC
  public static int Port { get; private set; }

  public LocalHttpServer()
  {
    var builder = WebApplication.CreateSlimBuilder();
    var logger = LoggerInitializer.CreateLoggerConfiguration("system", true);
    builder.Services
      .AddSerilog(logger)
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
    _app.MapGrpcService<UserUtilService>();
  }
  
  private ulong CreateTcpListenerHandle()
  {
    _tcpListener = new TcpListener(IPAddress.Loopback, Constants.Debug ? Constants.TestSystemServicePort : 0);
    _tcpListener.Start();
    Port = ((IPEndPoint)_tcpListener.LocalEndpoint).Port;
    return (ulong)_tcpListener.Server.Handle.ToInt64();
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
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