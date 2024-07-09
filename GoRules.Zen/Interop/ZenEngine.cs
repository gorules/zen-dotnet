using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace GoRules.Zen;

public class ZenEngine : IDisposable
{
  private readonly unsafe ZenEngineStruct* _internalReference;
  private readonly GCHandle? _callbackHandle;

  private bool _disposed;

  public ZenEngine(ZenEngineOptions? options = null)
  {
    var loaderDelegate = options?.GetLoaderDelegate();
    if (loaderDelegate is null)
    {
      unsafe
      {
        _internalReference = ZenFfi.zen_engine_new();
      }

      return;
    }

    unsafe
    {
      _callbackHandle = GCHandle.Alloc(loaderDelegate);
      IntPtr pCallback = Marshal.GetFunctionPointerForDelegate(loaderDelegate);
      _internalReference = ZenFfi.zen_engine_new_with_native_loader((delegate* unmanaged[Cdecl]<byte*, ZenDecisionLoaderResult>)pCallback);
    }
  }

  public async Task<ZenDecision> GetDecision(string key)
  {
    return await Task.Run(() =>
    {
      unsafe
      {
        fixed (byte* keyPtr = Encoding.UTF8.GetBytes(key))
        {
          var result = ZenFfi.zen_engine_get_decision(_internalReference, keyPtr);
          var safeResult = SafeZenResult<ZenDecision>.FromResult(result);
          return safeResult.Result;
        }
      }
    }).ConfigureAwait(false);
  }

  public ZenDecision CreateDecision(byte[] jsonContent)
  {
    unsafe
    {
      fixed (byte* jsonContentPtr = jsonContent)
      {
        var result = ZenFfi.zen_engine_create_decision(_internalReference, jsonContentPtr);
        var safeResult = SafeZenResult<ZenDecision>.FromResult(result);
        return safeResult.Result;
      }
    }
  }

  public async Task<ZenEvaluationResult<T>> Evaluate<T>(string key, dynamic context, ZenEvaluationOptions? options = null)
  {
    options ??= new ZenEvaluationOptions();

    var result = await Task.Run(() =>
    {
      unsafe
      {
        fixed (byte* keyPtr = Encoding.UTF8.GetBytes(key))
        {
          string jsonContext = JsonSerializer.Serialize(context, JsonOptions.Default);
          fixed (byte* jsonContextPtr = Encoding.UTF8.GetBytes(jsonContext))
          {
            var result = ZenFfi.zen_engine_evaluate(_internalReference, keyPtr, jsonContextPtr, options.ToFfi());
            return SafeZenResult<string>.FromResult(result);
          }
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

  ~ZenEngine()
  {
    DisposeResource();
  }
  
  private void DisposeResource()
  {
    if (_disposed) return;
    
    unsafe
    {
      ZenFfi.zen_engine_free(_internalReference);
      if (_callbackHandle is not null && _callbackHandle.Value.IsAllocated)
      {
        _callbackHandle.Value.Free();
      }
    }

    _disposed = true;
  }
}
