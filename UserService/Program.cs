using Microsoft.Win32;
using Serilog;
using SparkerUserService;
using SparkerUserService.Preferences;
using SparkerUserService.Utils;

LoggerInitializer.Initialize();
Preference.InitializeAllPreferences();

Log.Information("IsInUserSession: {inUserSession}, IsInteractive: {interactive}, IsElevated: {elevated}", 
  SessionChecker.IsInUserSession(), 
  SessionChecker.IsInteractive(),
  SessionChecker.IsElevated()
);

SystemEvents.SessionSwitch += new SessionSwitchEventHandler((sender, eventArgs) =>
{
  switch (eventArgs.Reason)
  {
    case SessionSwitchReason.SessionLogon:
      Task.Run(async () =>
      {
        if (await TrayIconManager.TryInitialize()) Utils.WelcomeMessage();
      });
      break;
  }
});

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
var host = builder.Build();
host.Run();
