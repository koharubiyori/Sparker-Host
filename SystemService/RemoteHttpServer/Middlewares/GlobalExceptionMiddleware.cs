using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Serilog;
using SparkerSystemService.RemoteHttpServer.Utils;

namespace SparkerSystemService.RemoteHttpServer.Middlewares;

public static class GlobalExceptionMiddlewareExtensions
{
  public static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder builder)
  {
    return builder.Use(async (context, next) =>
    {
      try
      {
        await next(context);
      }
      catch (Exception ex)
      {
        await HandleExceptionAsync(context, ex);
      }
    });
  }
  
  private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
  {
    context.Response.ContentType = "application/json";
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
      
    if (exception is JsonException or InvalidOperationException)
    {
      context.Response.StatusCode = StatusCodes.Status400BadRequest;
      var error = new ResponseData<string>(ErrorCode.ParameterError, "Invalid request body format", exception.Message);
      await context.Response.WriteAsJsonAsync(error);
    }
    else if (exception.GetType().IsGenericType && exception.GetType().GetGenericTypeDefinition() == typeof(RemoteHttpServerException<>))
    {
      Log.Error(exception, "RemoteHttpServerException: {Message}", exception.Message);
      
      switch (exception)
      {
        case RemoteHttpServerException<string> ex:
          await context.Response.WriteAsJsonAsync(ex.ToResponseData());
          break;
        case RemoteHttpServerException<string[]> ex:
          await context.Response.WriteAsJsonAsync(ex.ToResponseData());
          break;
        case RemoteHttpServerException<Dictionary<string, string[]>> ex:
          await context.Response.WriteAsJsonAsync(ex.ToResponseData());
          break;
        case RemoteHttpServerException<ResponseData.Empty> ex:
          await context.Response.WriteAsJsonAsync(ex.ToResponseData());
          break;
      }
    }
    else
    {
      Log.Error(exception, "Unhandled exception");

      var result = new ResponseData<ResponseData.Empty>(ErrorCode.InternalError, "Internal Server Error", ResponseData.Empty.Value);
      await context.Response.WriteAsJsonAsync(result);
    }
  }
}

public class RemoteHttpServerException<T>(
  ErrorCode code = ErrorCode.UnknownError, 
  string message = "Unknown error", 
  T? dataObject = default
) : Exception(message)
{
  public ErrorCode Code { get; } = code;
  public T? DataObject { get; } = dataObject;

  public ResponseData<T?> ToResponseData()
  {
    return new ResponseData<T?>(Code, Message, DataObject);
  }
}