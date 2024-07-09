using System.Runtime.InteropServices;

namespace GoRules.Zen;

internal class SafeZenResult<T>
{
  public readonly T Result;

  private SafeZenResult(T r)
  {
    Result = r;
  }

  public static unsafe SafeZenResult<string> FromResult(ZenResult<byte> result)
  {
    var exception = ZenException.TryFromResult(result);
    if (exception is not null)
      throw exception;

    var data = Marshal.PtrToStringUTF8((IntPtr)result.result) ?? throw new InvalidOperationException("UTF8 String returned null");
    ZenFfi.free((IntPtr)result.result);

    return new SafeZenResult<string>(data);
  }

  public static unsafe SafeZenResult<ZenDecision> FromResult(ZenResult<ZenDecisionStruct> result)
  {
    var exception = ZenException.TryFromResult(result);
    if (exception is not null)
      throw exception;

    var decision = new ZenDecision(result.result);
    return new SafeZenResult<ZenDecision>(decision);
  }

  public static unsafe SafeZenResult<int> FromResult(ZenResult<Int32> result)
  {
    var exception = ZenException.TryFromResult(result);
    if (exception is not null)
      throw exception;

    var data = *result.result;
    ZenFfi.free((IntPtr)result.result);

    return new SafeZenResult<int>(data);
  }
}
