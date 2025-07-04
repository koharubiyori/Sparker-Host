#if !CRED
using System.IO.Pipes;
using Serilog;

namespace SparkerCommons.Utils;

public abstract class NamedPipeEndpoint<T>()
  where T : PipeStream
{
  protected readonly string pipeName;
  protected readonly string pipeLabel;
  protected readonly bool allowWriteBeforeConnection;
  protected readonly int maxMessageLength;

  private T? _pipeStream;
  private bool _stopFlag;
  private TaskCompletionSource _untilConnected;
  private CancellationTokenSource? _tokenToBreakReadLoop;

  protected virtual Task OnConnected() { return Task.CompletedTask; }
  
  protected NamedPipeEndpoint(
    string pipeName,
    string pipeLabel,
    bool allowWriteBeforeConnection = true,
    int maxMessageLength = 1024
  ) : this()
  {
    this.pipeName = pipeName;
    this.pipeLabel = pipeLabel;
    this.allowWriteBeforeConnection = allowWriteBeforeConnection;
    this.maxMessageLength = maxMessageLength;
  }

  public async Task RunAsync()
  {
    _stopFlag = false;
    while (!_stopFlag)
    {
      if (allowWriteBeforeConnection) _untilConnected = new TaskCompletionSource();
      if (typeof(T) == typeof(NamedPipeServerStream))
      {
        _pipeStream = (T?)(PipeStream)new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1,
          PipeTransmissionMode.Message, PipeOptions.Asynchronous);
      }
      else if (typeof(T) == typeof(NamedPipeClientStream))
      {
        _pipeStream = (T?)(PipeStream)new NamedPipeClientStream(".", pipeName, PipeDirection.InOut,
          PipeOptions.Asynchronous);
      }
      else
      {
        throw new Exception("Unsupported PipeStream type");
      }

      try
      {
        if (_pipeStream is NamedPipeServerStream server)
        {
          Log.Information("[{PipeLabel}] Waiting for the connection...", pipeLabel);
          await server.WaitForConnectionAsync();
        }

        if (_pipeStream is NamedPipeClientStream client)
        {
          Log.Information("[{PipeLabel}] Connecting...", pipeLabel);
          await client.ConnectAsync();
        }
      }
      catch (Exception e)
      {
        if (_stopFlag) continue;
        Log.Information(e, "[{PipeLabel}] Failed to connect: {Message}", pipeLabel, e.Message);
        await Task.Delay(500);
        continue;
      }

      Log.Information("[{PipeLabel}] Connected!", pipeLabel);
      _tokenToBreakReadLoop = new CancellationTokenSource();
      if (allowWriteBeforeConnection) _untilConnected.SetResult();
      await OnConnected();
      
      await ReadLoop();
      await Clean();
    }
  }
  
  public async Task StopAsync()
  {
    _stopFlag = true;
    await Clean();
    Log.Information("[{PipeLabel}] Stopped!", pipeLabel);
  }

  protected virtual Task HandleMessage(string[] message)
  {
    var messageType = message[0];
    Log.Information("[{PipeLabel}] Received Message: {MessageType}", pipeLabel, messageType);
    return Task.CompletedTask;
  }

  private async Task ReadLoop()
  {
    Memory<byte> buffer = new byte[maxMessageLength];

    try
    {
      while (_pipeStream!.IsConnected && !_tokenToBreakReadLoop!.IsCancellationRequested)
      {
        // The parameter cancellationToken of ReadAsync() is unreliable! if you want to stop the loop, call Clean()
        var bytesRead = await _pipeStream!.ReadAsync(buffer, _tokenToBreakReadLoop!.Token);
        if (bytesRead == 0)
        {
          Log.Information("[{PipeLabel}] The connection is closed!", pipeLabel);
          return;
        }

        var unpackedMessages = StringArrayPacker.Unpack(buffer.ToArray());
        foreach (var message in unpackedMessages)
        {
          _ = HandleMessage(message);
        }
      }
    }
    catch (Exception e) when (e is IOException or ObjectDisposedException)
    {
      Log.Error(e, "[{PipeLabel}] The pipe has been closed or broken!", pipeLabel);
    }
    catch (OperationCanceledException e)
    {
      Log.Error(e, "[{PipeLabel}] Break the read loop manually!", pipeLabel);
    }
  }

  private async Task Clean()
  {
    if (_tokenToBreakReadLoop != null) await _tokenToBreakReadLoop.CancelAsync();
    if (_pipeStream is { IsConnected: true } and NamedPipeServerStream server) server.Disconnect();
    if (_pipeStream != null) await _pipeStream.DisposeAsync();
    _pipeStream = null;
    _tokenToBreakReadLoop = null;
  }

  protected async Task<bool> Write(params string[] items)
  {
    if (allowWriteBeforeConnection)
    {
      await _untilConnected.Task;
    }
    else if (_pipeStream is not { IsConnected: true })
    {
      Log.Error("[{PipeLabel}] The pipe has not connected yet!", pipeLabel);
      return false;
    }

    var packagedData = StringArrayPacker.Pack(items);
    try
    {
      using var cts = new CancellationTokenSource(1000);
      await _pipeStream!.WriteAsync(packagedData, cts.Token);
      await _pipeStream.FlushAsync(cts.Token);
      return true;
    }
    catch (Exception e) when (e is OperationCanceledException or IOException)
    {
      Log.Error(e, "[{PipeLabel}] Failed to write. The pipe has been closed or broken!", pipeLabel);
      await Clean();
      return false;
    }
  }
}
#endif