// using Microsoft.Win32;
// using Serilog;
// using SparkerUserService;
// using SparkerUserService.Preferences;
// using SparkerUserService.Utils;
//
// LoggerInitializer.Initialize();
// Preference.InitializeAllPreferences();
//
// Log.Information("IsInUserSession: {inUserSession}, IsInteractive: {interactive}, IsElevated: {elevated}", 
//   SessionChecker.IsInUserSession(), 
//   SessionChecker.IsInteractive(),
//   SessionChecker.IsElevated()
// );
//
// var builder = Host.CreateApplicationBuilder(args);
// builder.Services.AddHostedService<Worker>();
// var host = builder.Build();
// host.Run();
