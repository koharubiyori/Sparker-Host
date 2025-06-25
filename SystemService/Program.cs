using System.ServiceProcess;
using SparkerSystemService;
using SparkerSystemService.LocalServices;
using SparkerSystemService.Pipes;
using SparkerSystemService.Utils;

LoggerInitializer.Initialize();
// Preference.InitializeAllPreferences();
var localServer = new LocalServer();

if (Environment.UserInteractive)
{
  Task.WaitAll(
    PipeToCred.Instance.RunAsync(),
    PipeToServer.Instance.RunAsync(),
    PipeToUserService.Instance.RunAsync(),
    // ChildServiceLauncher.RunAsync(),
    localServer.RunAsync()
  );
}
else
{
  ServiceBase.Run(new SparkerService(localServer));
}