namespace GoRules.Zen;

public record ZenEvaluationResult<T>
{
  public required string Performance { get; init; }

  public required T Result { get; init; }

  public object? Trace { get; init; }
}
