// using System.ServiceProcess;
// using Serilog;
// using SparkerSystemService.LocalServices;
// using SparkerSystemService.Pipes;
// using SparkerSystemService.TinyUtils;
//
// namespace SparkerSystemService;
//
// public class SparkerService : ServiceBase
// {
//   private LocalServer _localServer;
//   public static Action Clean { get; private set; }
//   public static Action Stop { get; private set; }
//
//   public SparkerService()
//   {
//     LoggerInitializer.Initialize();
//     _localServer = new LocalServer();
//     
//     ServiceName = "SunshineService";
//     CanStop = true;
//     CanHandleSessionChangeEvent = true;
//
//     Clean = _Clean;
//     Stop = _Stop;
//   }
//
//   private void _Clean()
//   {
//     ChildServiceLauncher.Stop();
//     Task.WaitAll(
//       PipeToCred.Instance.StopAsync(),
//       PipeToServer.Instance.StopAsync(),
//       PipeToUserService.Instance.StopAsync(),
//       _localServer.StopAsync()
//     );
//   }
//
//   private void _Stop()
//   {
//     base.Stop();
//   }
//
//   protected override void OnStart(string[] args)
//   {
//     Log.Information("Starting SparkerService");
//     _ = PipeToCred.Instance.RunAsync();
//     _ = PipeToServer.Instance.RunAsync();
//     _ = PipeToUserService.Instance.RunAsync();
//     _ = ChildServiceLauncher.RunAsync();
//     _ = _localServer.RunAsync();
//   }
//
//   protected override void OnSessionChange(SessionChangeDescription changeDescription)
//   {
//     switch (changeDescription.Reason)
//     {
//       case SessionChangeReason.SessionLock:
//         Log.Information("Session lock!");
//         PowerManager.SetLocked(true);
//         break;
//
//       case SessionChangeReason.SessionUnlock:
//         Log.Information("Session unlock!");
//         PowerManager.SetLocked(false);
//         break;
//     }
//   }
//
//   protected override void OnStop()
//   {
//     _Clean();
//   }
//
//   public static async Task DebugRun()
//   {
//     var localServer = new LocalServer();
//     await Task.WhenAll(
//       PipeToCred.Instance.RunAsync(),
//       PipeToServer.Instance.RunAsync(),
//       PipeToUserService.Instance.RunAsync(),
//       localServer.RunAsync()
//     );
//   }
// }
