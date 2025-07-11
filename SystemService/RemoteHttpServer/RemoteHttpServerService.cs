using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog;
using ServiceShared.Utils;
using SparkerCommons;
using SparkerSystemService.RemoteHttpServer.Filter;
using SparkerSystemService.RemoteHttpServer.Middlewares;
using SparkerSystemService.RemoteHttpServer.Routes;

namespace SparkerSystemService.RemoteHttpServer;

public class RemoteHttpServerService : BackgroundService
{
  private readonly WebApplication _app;

  public RemoteHttpServerService()
  {
    var builder = WebApplication.CreateSlimBuilder();
    var port = PortFinder.FindFreePort(Constants.ServerServicePort);
    var logger = LoggerInitializer.CreateLoggerConfiguration("system","RemoteHttpServer");
    builder.Services
      .AddSerilog(logger)
      .ConfigureHttpJsonOptions(static options =>
      {
        options.SerializerOptions.TypeInfoResolverChain.Insert(0, RemoteHttpServerJsonGenContext.Default);
      });
    _app = builder.Build();
    _app.Urls.Add($"http://0.0.0.0:{port}");
    UseMiddlewares();
    BindRoutes();
  }

  private void UseMiddlewares()
  {
    _app
      .UseWebSockets()
      // .UseGlobalExceptionMiddleware()
      .UseExceptionHandler(errorApp =>
      {
        errorApp.Run(async context =>
        {
          var feature = context.Features.Get<IExceptionHandlerFeature>();
          var exception = feature?.Error;
  
          Log.Error(exception.Message);
          context.Response.ContentType = "application/json";
          context.Response.StatusCode = StatusCodes.Status500InternalServerError;
  
          await context.Response.WriteAsJsonAsync(new
          {
            error = exception?.Message ?? "Unknown error"
          });
        });
      })
      // .UseAuthMiddleware(["/device/getPairingCode", "/device/pair", "/hostInfo/approach"])
    ;
  }
    
  private void BindRoutes()
  {
    // HostInfo
    _app.MapPost("/hostInfo/approach", HostInfoRoute.Approach)
      .AddEndpointFilter<ValidationFilter<HostInfoRoute.ApproachRequest>>();
    _app.MapPost("/hostInfo/getBasicInfo", HostInfoRoute.GetBasicInfo);
    // Power
    _app.MapPost("/power/shutdown", PowerRoute.Shutdown)
      .AddEndpointFilter<ValidationFilter<PowerRoute.ShutdownRequest>>();
    _app.MapPost("/power/sleep", PowerRoute.Sleep)
      .AddEndpointFilter<ValidationFilter<PowerRoute.SleepRequest>>();
    _app.MapPost("/power/lock", PowerRoute.Lock);
    _app.MapPost("/power/unlock", (Delegate)PowerRoute.Unlock);
    _app.MapPost("/power/isLocked", PowerRoute.IsLocked);
    // Device
    _app.MapPost("/device/getPairingCode", async (DeviceRoute.GetPairingCodeRequest request) => await DeviceRoute.GetPairingCode(request))
      .AddEndpointFilter<ValidationFilter<DeviceRoute.GetPairingCodeRequest>>();
    _app.MapPost("/device/pair", DeviceRoute.Pair)
      .AddEndpointFilter<ValidationFilter<DeviceRoute.PairRequest>>();
    _app.MapPost("/device/unpair", DeviceRoute.Unpair)
      .AddEndpointFilter<ValidationFilter<DeviceRoute.UnpairRequest>>();
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    await _app.RunAsync(stoppingToken);
  }
}