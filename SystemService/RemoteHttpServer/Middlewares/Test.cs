using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog;
using SparkerSystemService.RemoteHttpServer.Utils;

namespace SparkerSystemService.RemoteHttpServer.Middlewares;

public static class GlobalExceptionHandleraExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandlera(this IApplicationBuilder app)
    {
        return app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(HandleExceptionAsync);
        });
    }

    private static async Task HandleExceptionAsync(HttpContext context)
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        Log.Warning(exception?.ToString());

        context.Response.ContentType = "application/json";

        if (exception is JsonException or InvalidOperationException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            var error = new ResponseData<string>(
                ErrorCode.ParameterError,
                "Invalid request body format",
                exception?.Message ?? "Invalid JSON");

            await context.Response.WriteAsJsonAsync(
                error,
                RemoteHttpServerJsonGenContext.Default.ResponseDataString);
        }
    }
}