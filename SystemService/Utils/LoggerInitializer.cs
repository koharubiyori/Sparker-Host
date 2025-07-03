using Commons;
using Serilog;
using Serilog.Enrichers.WithCaller;

namespace SparkerSystemService.Utils;

public static class LoggerInitializer
{
  public static void Initialize()
  {
    var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 60;
    var loggerConfiguration = new LoggerConfiguration()
      .Enrich.WithCaller()
      .WriteTo.File(
        path: Path.Combine(Constants.LogDirPath, $"system_{currentTimestamp}.log")
      );

#if DEBUG
    loggerConfiguration
      .MinimumLevel.Debug()
      .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message} {Caller}{NewLine}{Exception}",
        theme: AnsiConsoleTheme.Code, 
        applyThemeToRedirectedOutput: true
      );
#else
    // loggerConfiguration
    //   .MinimumLevel.Error();
#endif
  
    Log.Logger = loggerConfiguration.CreateLogger();
  
    AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) =>
    {
      if (e.ExceptionObject is Exception exception)
      {
        Log.Error(exception, "Unhandled exception");
      }
    };
    
    TaskScheduler.UnobservedTaskException += (s, e) =>
    {
      Log.Error(e.Exception, "Unhandled exception");
      e.SetObserved();
    };
  }
}