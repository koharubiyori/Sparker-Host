using System.ServiceProcess;
using Serilog;
using ServiceShared;
using SparkerSystemService;
using SparkerUserService;

#if !DEBUG
Console.WriteLine("Logging is disabled in release mode.");
#endif

var moduleToRun = args[0];   // system or user
var isSystemModule = moduleToRun == "system";

if (isSystemModule)
  SystemServiceModule.Initialize();
else
  UserServiceModule.Initialize();

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddSerilog();
builder.Services
  .Configure<LoggerFilterOptions>(options =>
  {
    options.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
  })
  .AddSingleton<WindowsServiceAdapter>(sp =>
    new WindowsServiceAdapter(sp.GetRequiredService<IHost>(), "Sparker Service"));

if (isSystemModule)
  builder.Services.AddSparkerSystemService();
else
  builder.Services.AddSparkerUserService();

var host = builder.Build();

var windowsServiceAdapter = host.Services.GetRequiredService<WindowsServiceAdapter>();

if (Environment.UserInteractive)
{
  host.RunAsync().Wait();
}
else
{
  ServiceBase.Run(windowsServiceAdapter);
}