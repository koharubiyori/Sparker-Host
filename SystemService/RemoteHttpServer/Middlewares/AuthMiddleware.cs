using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Serilog;
using SparkerSystemService.Preferences;
using SparkerSystemService.RemoteHttpServer.Utils;

namespace SparkerSystemService.RemoteHttpServer.Middlewares;

public static class AuthMiddlewareExtensions
{
  public static IApplicationBuilder UseAuthMiddleware(this IApplicationBuilder builder, string[] excludePaths)
  {
    return builder.Use(async (context, next) =>
    {
      if (excludePaths.Contains(context.Request.Path.Value!))
      {
        await next(context);
        return;
      }

      var token = context.Request.Headers.Authorization.ToString();

      if (!await AuthorizationHelper.IsAuthorized(token))
      {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return;
      }

      await next(context);
    });
  }
}
public static class AuthorizationHelper
{
  public static async Task<bool> IsAuthorized(string? token)
  {
    if (string.IsNullOrWhiteSpace(token))
      return false;

    try
    {
      var decoded = await Tokener.DecodeAsync(token);
      return Preference.PairedDevice.Exists(decoded.DeviceId);
    }
    catch (Exception ex)
    {
      Log.Error(ex, "Failed to authorize");
      return false;
    }
  }
}