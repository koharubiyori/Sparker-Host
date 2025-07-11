using SparkerCommons;
using Serilog;
using Serilog.Enrichers.WithCaller;
using Serilog.Sinks.SystemConsole.Themes;

namespace ServiceShared.Utils;

public static class LoggerInitializer
{
  public static ILogger CreateLoggerConfiguration(string logFilePrefix, bool withWebHostLabel = false)
  {
    var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 60;
    var loggerConfiguration = new LoggerConfiguration()
      .Enrich.WithCaller()
      .WriteTo.File(
        path: Path.Combine(Constants.LogDirPath, $"{logFilePrefix}_{currentTimestamp}.log")
      );
    var webHostLabel = withWebHostLabel ? " ({WebHostLabel})" : "";
    var outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}]" + webHostLabel + " {Message} {Caller}{NewLine}{Exception}";

    if (withWebHostLabel) loggerConfiguration.Enrich.WithProperty("WebHostLabel", "WebHost");
    
#if DEBUG
    loggerConfiguration
      .MinimumLevel.Debug()
      .WriteTo.Console(
        outputTemplate: outputTemplate,
        theme: AnsiConsoleTheme.Code, 
        applyThemeToRedirectedOutput: true
      );
#else
    loggerConfiguration
      .MinimumLevel.Error();
#endif

    return loggerConfiguration.CreateLogger();
  }

  public static void InitializeGlobalLogger(ILogger logger)
  {
    Log.Logger = logger;
    
    AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) =>
    {
      if (e.ExceptionObject is Exception exception)
      {
        Log.Error(exception, "Unhandled exception");
      }
    };
    
    TaskScheduler.UnobservedTaskException += (s, e) =>
    {
      Log.Error(e.Exception, "Unobserved exception");
      e.SetObserved();
    };
  }
}