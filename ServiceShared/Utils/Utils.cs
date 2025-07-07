using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ServiceShared.Utils;

public static class Utils
{
  public static IServiceCollection AddOpenedHostedService
    <[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>
    (this IServiceCollection collection) where T : class, IHostedService
  {
    return collection
      .AddSingleton<T>()
      .AddHostedService(sp => sp.GetRequiredService<T>());
  }
}