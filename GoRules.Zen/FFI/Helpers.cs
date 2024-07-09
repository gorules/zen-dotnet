using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace GoRules.Zen;

internal static class Helpers
{
  public static IntPtr AllocString(byte[] data)
  {
    IntPtr unmanagedPointer = Marshal.AllocHGlobal(data.Length + 1); // +1 for null terminator

    Marshal.Copy(data, 0, unmanagedPointer, data.Length);
    Marshal.WriteByte(unmanagedPointer + data.Length, 0); // Add NULL terminated

    return unmanagedPointer;
  }

  public static IntPtr AllocString(string data)
  {
    return AllocString(Encoding.UTF8.GetBytes(data) ?? throw new InvalidOperationException("String could not be converted to bytes"));
  }
}

internal static class JsonOptions
{
  public static readonly JsonSerializerOptions Default = new JsonSerializerOptions
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
  };
}
