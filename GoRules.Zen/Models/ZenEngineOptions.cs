using System.Runtime.InteropServices;

namespace GoRules.Zen;

public record ZenEngineOptions
{
  public Func<string, Task<byte[]>>? Loader;

  internal unsafe ZenEngineCallbackDelegate? GetLoaderDelegate()
  {
    if (Loader is null)
      return null;

    return keyPtr =>
    {
      try
      {
        var key = Marshal.PtrToStringUTF8((IntPtr)keyPtr)
                  ?? throw new InvalidOperationException("UTF8 String is null");

        var jsonContent = Loader(key).Result;
        var jsonPointer = Helpers.AllocString(jsonContent);

        return new ZenDecisionLoaderResult
        {
          content = (byte*)jsonPointer.ToPointer()
        };
      }
      catch (Exception e)
      {
        var errorMessage = $"{e.Message}";
        var errorPointer = Helpers.AllocString(errorMessage);

        return new ZenDecisionLoaderResult
        {
          error = (byte*)errorPointer.ToPointer()
        };
      }
    };
  }
}
