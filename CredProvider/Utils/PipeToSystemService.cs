using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Commons;
using Commons.Utils;

namespace SparkerCredProvider.Utils
{
  public static class PipeToSystemService
  {
    private const string PipeLabel = "PipeToSystemService";
    private static readonly List<PipeClientHandler> _clients = new List<PipeClientHandler>();
    private static readonly object _clientLock = new object();
    private static CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    public static Action<string, string> OnUserInfoReceived { get; set; }

    public static async Task RunAsync()
    {
      Logger.Write($"{PipeLabel}: Starting to listen for client connections...");

      while (!_cancellationTokenSource.IsCancellationRequested)
      {
        Logger.Write("test");
        NamedPipeServerStream pipeServer = null;
        try
        {
          pipeServer = new NamedPipeServerStream(
            Constants.CredPipeName,
            PipeDirection.InOut,
            NamedPipeServerStream.MaxAllowedServerInstances,
            PipeTransmissionMode.Message,
            PipeOptions.Asynchronous);

          Logger.Write($"{PipeLabel}: Waiting for a new connection...");

          using (_cancellationTokenSource.Token.Register(() => pipeServer.Dispose()))
          {
            await pipeServer.WaitForConnectionAsync(_cancellationTokenSource.Token);
          }

          var handler = new PipeClientHandler(pipeServer, OnUserInfoReceived);
          lock (_clientLock)
          {
            _clients.Add(handler);
          }

          var unused = handler.RunAsync(() =>
          {
            lock (_clientLock)
            {
              _clients.Remove(handler);
            }
          });

          Logger.Write($"{PipeLabel}: Client connected. Total clients: {_clients.Count}");
        }
        catch (OperationCanceledException)
        {
          Logger.Write($"{PipeLabel}: The listener loop was canceled.");
          break;
        }
        catch (Exception ex)
        {
          if (_cancellationTokenSource.IsCancellationRequested)
          {
            Logger.Write($"{PipeLabel}: The listener loop was canceled.\n{ex}");
            break;
          }

          Logger.Write($"{PipeLabel}: An error occurred in the listener loop. Retrying in 1 second.");
          pipeServer?.Dispose();
          await Task.Delay(1000, _cancellationTokenSource.Token);
        }
      }
    }

    public static void Stop()
    {
      _cancellationTokenSource.Cancel();
    }

    public static Task WriteUnlockRequestToAllClients()
    {
      lock (_clientLock)
      {
        Logger.Write($"{PipeLabel}: Broadcasting unlock request to {_clients.Count} clients.");
        var clientCopy = _clients.ToArray();
        var writeTasks = clientCopy.Select(client => client.WriteUnlockRequestAsync());
        return Task.WhenAll(writeTasks);
      }
    }

    public static Task DisposeAsync()
    {
      if (_cancellationTokenSource == null) return Task.FromResult(0);

      Logger.Write($"{PipeLabel}: Disposing and stopping all connections...");
      _cancellationTokenSource.Cancel();

      PipeClientHandler[] clientCopy;
      lock (_clientLock)
      {
        clientCopy = _clients.ToArray();
      }

      var shutdownTasks = clientCopy.Select(client => client.DisposeAsync());
      var whenAllTask = Task.WhenAll(shutdownTasks);

      whenAllTask.ContinueWith(t =>
      {
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = null;
      });

      return whenAllTask;
    }

    private class PipeClientHandler
    {
      private readonly NamedPipeServerStream _pipeStream;
      private readonly Action<string, string> _onUserInfoReceived;

      public PipeClientHandler(NamedPipeServerStream pipeStream, Action<string, string> onUserInfoReceived)
      {
        _pipeStream = pipeStream;
        _onUserInfoReceived = onUserInfoReceived;
      }

      public async Task RunAsync(Action onDisconnected)
      {
        try
        {
          await ReadLoopAsync();
        }
        catch (Exception ex)
        {
          Logger.Write($"{PipeLabel}: An unhandled exception occurred in the read loop.\n{ex}");
        }
        finally
        {
          onDisconnected?.Invoke();
          await DisposeAsync();
        }
      }

      private async Task ReadLoopAsync()
      {
        var buffer = new byte[1024];
        while (_pipeStream.IsConnected)
        {
          var bytesRead = await _pipeStream.ReadAsync(buffer, 0, buffer.Length);
          if (bytesRead == 0) break;

          var receivedData = new byte[bytesRead];
          Array.Copy(buffer, 0, receivedData, 0, bytesRead);
          var unpackedMessages = StringArrayPacker.Unpack(receivedData);

          foreach (var message in unpackedMessages)
          {
            await HandleMessageAsync(message);
          }
        }
      }

      private Task HandleMessageAsync(string[] message)
      {
        var messageType = message[0];
        Logger.Write($"{PipeLabel}: Received Message: {messageType}");

        switch (messageType)
        {
          case Constants.CredPipeMessageType.In.Logon:
            var username = message[1];
            var domain = message[2].Length == 0 ? Environment.MachineName : message[2];
            var password = message[3];
            _onUserInfoReceived?.Invoke($"{domain}\\{username}", password);
            break;
          case Constants.CredPipeMessageType.In.Ping:
            return WritePongAsync();
        }

        return Task.FromResult(0);
      }

      public Task WriteUnlockRequestAsync()
      {
        return WriteAsync(Constants.CredPipeMessageType.Out.RequestToUnlock)
          .ContinueWith(t => Logger.Write($"{PipeLabel}: Sent unlock request!"));
      }

      private Task WritePongAsync()
      {
        return WriteAsync(Constants.CredPipeMessageType.Out.Pong)
          .ContinueWith(t => Logger.Write($"{PipeLabel}: Sent pong!"));
      }

      private async Task WriteAsync(params string[] items)
      {
        if (!_pipeStream.IsConnected) return;
        var packagedData = StringArrayPacker.Pack(items);
        try
        {
          await _pipeStream.WriteAsync(packagedData, 0, packagedData.Length);
          await _pipeStream.FlushAsync();
        }
        catch (IOException ex)
        {
          Logger.Write($"{PipeLabel}: Failed to write. The pipe has been closed or broken.\n{ex}");
        }
      }

      public Task DisposeAsync()
      {
        if (_pipeStream.IsConnected)
        {
          try
          {
            _pipeStream.Disconnect();
          }
          catch (Exception ex)
          {
            Logger.Write($"{PipeLabel}: Error during disconnect.\n{ex}");
          }
        }

        _pipeStream.Dispose();
        return Task.FromResult(0);
      }
    }
  }
}