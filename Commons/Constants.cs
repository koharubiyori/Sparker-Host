using System.IO;
using Microsoft.Win32;

namespace SparkerCommons
{
  public static class Constants
  {
    public const string CredPipeName = "098F7CB5-41A9-4A20-803B-63BA7497B814";
    public const string ServerPipeName = "38A2CC49-97C8-49E5-BEF9-93536F9A9D15";
    public const string SystemServicePipeName = "107156F9-69CA-4812-BB2D-1FCA23324F38";
    
    public static class CredPipeMessageType
    {
      public static class In
      {
        public const string Ping = "ping";  // Test the connection if it is alive
        public const string Logon = "logon";  // Handle the logon request. Arguments: username, password
      }
      public static class Out
      {
        public const string Pong = "pong";  // Relay the ping message 
        public const string RequestToUnlock = "requestToUnlock"; // Request the remote end to send a logon request
      }
    }
    
    public static class ServerPipeMessageType
    {
      public static class In
      {
        public const string SubmitPort = "submitPort"; // Receive the dynamic port number in use of the SystemService and UserService. Arguments: "system" | "user", port
        public const string Stop = "stop"; // Clean and stop the Server itself. The Server will have no chance to clean itself if CloseHandle() is called at first
        public const string RequestToUnlock = "requestToUnlock"; // Cred -> SystemService -> Server
      }
    }

    public static class SystemServicePipeEvents
    {
      public static class In
      {
        public const string Restart = "restart";  // Restart the SystemService itself
        public const string Stop = "stop";  // Clean and Stop the SystemService itself
      }
    }

    public const string GithubUrl = "https://github.com/koharubiyori/Sparker-Host";
    public const string ServiceName = "Sparker";

    private const string PreferenceDirPathName = "Preferences";
    private const string LogDirPathName = "Logs";
    private const string ServerExeName = "Sparker.Server.exe";
    private const string UserServiceExeName = "Sparker.UserService.exe";

    public const int TestSystemServicePort = 13002;
    public const int TestUserServicePort = 13003;

#if DEBUG
    public const bool Debug = true;
    public static readonly string BasePath = Path.Combine(Directory.GetCurrentDirectory(), "GenBase");
    public static readonly string PreferenceDirPath = Path.Combine(BasePath, PreferenceDirPathName);
    public static readonly string LogDirPath = Path.Combine(BasePath, LogDirPathName);
    public static readonly string ServerExePath = Path.Combine(BasePath, ServerExeName);
    public static readonly string UserServiceExePath = Path.Combine(BasePath, UserServiceExeName);
#else
    public const bool Debug = false;
#if CRED
    public static readonly string BasePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
#else
    public static readonly string BasePath = Path.GetDirectoryName(Environment.ProcessPath)!;
#endif
    public static readonly string PreferenceDirPath = Path.Combine(BasePath, PreferenceDirPathName);
    public static readonly string LogDirPath = Path.Combine(BasePath, LogDirPathName);
    public static readonly string ServerExePath = Path.Combine(BasePath, ServerExeName); 
    public static readonly string UserServiceExePath = Path.Combine(BasePath, UserServiceExeName);
#endif
  }
}