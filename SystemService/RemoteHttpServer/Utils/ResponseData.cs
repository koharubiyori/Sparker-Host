namespace SparkerSystemService.RemoteHttpServer.Utils;

public record ResponseData<T>(ErrorCode Code, string Message, T Data);

public static class ResponseData
{
  public class Empty
  {
    public static readonly Empty Value = new Empty();
  }
  
  public static ResponseData<T> Success<T>(T data)
  {
    return new ResponseData<T>(ErrorCode.Success, "success", data);
  }

  public static ResponseData<Empty> Success()
  {
    return new ResponseData<Empty>(ErrorCode.Success, "success", Empty.Value);
  }

  public static ResponseData<Empty> WithoutData(ErrorCode errorCode, string message)
  {
    return new ResponseData<Empty>(errorCode, message, Empty.Value);
  }
}