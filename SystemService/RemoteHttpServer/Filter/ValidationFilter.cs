using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Serilog;
using SparkerSystemService.RemoteHttpServer.Utils;

namespace SparkerSystemService.RemoteHttpServer.Filter;

public class ValidationFilter<T> : IEndpointFilter where T : class
{
  public async ValueTask<object> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
  {
    if (context.Arguments.FirstOrDefault() is not T model)
    {
      Log.Warning("Validation failed for {ModelType}: Invalid request data.", typeof(T).Name);
      return ResponseData.WithoutData(ErrorCode.ParameterError, "Invalid request data.");
    }

    var validationContext = new ValidationContext(model);
    var validationResults = new List<ValidationResult>();

    if (!Validator.TryValidateObject(model, validationContext, validationResults, true))
    {
      var errors = validationResults
        .GroupBy(r => r.MemberNames.FirstOrDefault() ?? "")
        .ToDictionary(g => g.Key, g => g.Select(r => r.ErrorMessage ?? "").ToArray());

      Log.Warning("Validation failed for {ModelType}: {Errors}", typeof(T).Name, errors);
      return new ResponseData<Dictionary<string, string[]>>(ErrorCode.ParameterError, "Invalid request data.", errors);
    }

    
    return await next(context);
  }
}