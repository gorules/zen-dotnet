using System.Runtime.InteropServices;

namespace GoRules.Zen;

public class ZenException : Exception
{
  private readonly int _error;
  private readonly string? _details;

  private ZenException(int error, string? details = null)
  {
    _error = error;
    _details = details;
  }

  internal static unsafe ZenException? TryFromResult<T>(ZenResult<T> result)
  {
    if (result.error == 0)
      return null;

    if (result.details is null)
      return new ZenException(result.error);

    var errorDetails = Marshal.PtrToStringUTF8((IntPtr)result.details);
    ZenFfi.free((IntPtr)result.details);

    return new ZenException(result.error, errorDetails);
  }

  internal static ZenException FromResult<T>(ZenResult<T> result)
  {
    return TryFromResult(result) ?? throw new NullReferenceException("Expected a non-null reference");
  }
}
