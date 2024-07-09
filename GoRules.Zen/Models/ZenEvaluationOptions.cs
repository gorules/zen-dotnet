namespace GoRules.Zen;

public record ZenEvaluationOptions
{
  public bool Trace { get; init; } = false;
  public int MaxDepth { get; init; } = 5;

  internal ZenEngineEvaluationOptions ToFfi()
  {
    return new ZenEngineEvaluationOptions()
    {
      trace = Trace,
      max_depth = (byte)MaxDepth,
    };
  }
}
