using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading.Tasks;
using SparkerCommons;
using SparkerCommons.Utils;

namespace SparkerCredProvider.Utils
{
  public static class PipeToSystemService
  {
    private const string PipeLabel = "PipeToSystemService";
    public static Action<string, string> OnUserInfoReceived = null;

    private static NamedPipeServerStream _pipeServer;
    private static bool _stopFlag;

    public static async Task Run()
    {
      _stopFlag = false;
      while (!_stopFlag)
      {
        _pipeServer = new NamedPipeServerStream(Constants.CredPipeName, PipeDirection.InOut, 1,
          PipeTransmissionMode.Message, PipeOptions.Asynchronous);
        
        try
        {
          Logger.Write($"{PipeLabel}: Waiting for the connection...");
          await _pipeServer.WaitForConnectionAsync();
          Logger.Write($"{PipeLabel}: Connected!");
        }
        catch (Exception ex)
        {
          if (_stopFlag) continue;
          Logger.Write($"{PipeLabel}: Failed to connect: {ex.Message}");
          await Task.Delay(500);
          continue;
        }
        
        await ReadLoop();
      }
    }

    public static void Stop()
    {
      _stopFlag = true;
      Clean();
    }

    private static async Task WritePong()
    {
      await Write(Constants.CredPipeMessageType.Out.Pong);
      Logger.Write($"{PipeLabel}: Sent pong!");
    }
    
    public static async Task WriteUnlockRequest()
    {
      await Write(Constants.CredPipeMessageType.Out.RequestToUnlock);
      Logger.Write($"{PipeLabel}: Sent unlock request!");
    }

    private static async Task ReadLoop()
    {
      var buffer = new byte[1024];

      try
      {
        while (true)
        {
          var bytesRead = await _pipeServer.ReadAsync(buffer, 0, buffer.Length);
          if (bytesRead == 0)
          {
            Logger.Write($"{PipeLabel}: The connection is closed!");
            return;
          }
          
          var unpackedMessages = StringArrayPacker.Unpack(buffer.Take(bytesRead).ToArray());
          foreach (var message in unpackedMessages)
          {
            if (await HandleMessage(message)) return;
            await _pipeServer.FlushAsync();
          }
        }
      }
      catch (Exception e)
      {
        if (!(e is IOException) && !(e is OperationCanceledException) && !(e is ObjectDisposedException)) throw;
        Logger.Write($"{PipeLabel}: Pipe closed or broken!");
      }
    }

    private static async Task<bool> HandleMessage(string[] message)
    {
      var messageType = message[0];
      Logger.Write($"{PipeLabel}: Received Message: {messageType}");
          
      switch (messageType)
      {
        case Constants.CredPipeMessageType.In.Logon:
        {
          var username = message[1];
          var password = message[2];
          OnUserInfoReceived?.Invoke(username, password);
          break;
        }
        case Constants.CredPipeMessageType.In.Ping:
          await WritePong();
          break;
      }

      return false;
    }

    private static void Clean()
    {
      if (_pipeServer != null)
      {
        if (_pipeServer.IsConnected) _pipeServer.Disconnect();
        _pipeServer.Dispose();
      }
      
      _pipeServer = null;
    }

    private static async Task Write(params string[] items)
    {
      var packagedData = StringArrayPacker.Pack(items);
      try
      {
        await _pipeServer.WriteAsync(packagedData, 0, packagedData.Length);
        await _pipeServer.FlushAsync();
      }
      catch (IOException e)
      {
        Logger.Write($"{PipeLabel}: The pipe has been closed or broken!");
        Clean();
      }
    }
  }
}