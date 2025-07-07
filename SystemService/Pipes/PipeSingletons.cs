namespace SparkerSystemService.Pipes;

public static class PipeSingletons
{
  public static PipeToCred PipeToCred { get; set; }
  public static PipeToServer PipeToServer { get; set; }
  public static PipeToUserService PipeToUserService { get; set; }
}