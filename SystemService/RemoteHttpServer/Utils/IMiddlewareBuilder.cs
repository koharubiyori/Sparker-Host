using Microsoft.AspNetCore.Http;

namespace SparkerSystemService.RemoteHttpServer.Utils;

public interface IMiddlewareBuilder
{
  Func<HttpContext, RequestDelegate, Task<Task?>> Build();
}