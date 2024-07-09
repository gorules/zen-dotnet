using System.Text;
using System.Text.Json;

namespace GoRules.Zen;

public class ZenDecision : IDisposable
{
  private readonly unsafe ZenDecisionStruct* _internalReference;
  private bool _disposed;

  internal unsafe ZenDecision(ZenDecisionStruct* internalReference)
  {
    _internalReference = internalReference;
  }

  public async Task<ZenEvaluationResult<T>> Evaluate<T>(dynamic context, ZenEvaluationOptions? options = null)
  {
    options ??= new ZenEvaluationOptions();

    var result = await Task.Run(() =>
    {
      unsafe
      {
        string jsonContext = JsonSerializer.Serialize(context, JsonOptions.Default);
        fixed (byte* jsonContextPtr = Encoding.UTF8.GetBytes(jsonContext))
        {
          var result = ZenFfi.zen_decision_evaluate(_internalReference, jsonContextPtr, options.ToFfi());
          return SafeZenResult<string>.FromResult(result);
        }
      }
    });

    var jsonData = JsonSerializer.Deserialize<ZenEvaluationResult<T>>(result.Result, JsonOptions.Default);
    return jsonData ?? throw new InvalidOperationException("Deserialization returned null");
  }


  public void Dispose()
  {
    DisposeResource();
    GC.SuppressFinalize(this);
  }

  ~ZenDecision()
  {
    DisposeResource();
  }

  private void DisposeResource()
  {
    if (_disposed) return;

    unsafe
    {
      ZenFfi.zen_decision_free(_internalReference);
    }

    _disposed = true;
  }
}
