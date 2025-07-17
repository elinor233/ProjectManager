namespace Models.Base.Models;

public abstract class ResponseBase<T1, T2>
{
  public T1 Response { get; set; } = default!;
  public T2 Data { get; set; } = default!;
  public string Error { get; set; } = string.Empty;
}
