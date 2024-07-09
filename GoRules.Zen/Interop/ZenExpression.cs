using System.Text;
using System.Text.Json;

namespace GoRules.Zen;

public static class ZenExpression
{
  public static async Task<T> Evaluate<T>(string expression, dynamic? context = null)
  {
    var jsonData = await Task.Run(() =>
    {
      unsafe
      {
        fixed (byte* expressionPtr = Encoding.UTF8.GetBytes(expression))
        {
          string jsonContext = "null";
          if (context is not null)
          {
            jsonContext = JsonSerializer.Serialize(context, JsonOptions.Default);
          }
          
          fixed (byte* jsonContextPtr = Encoding.UTF8.GetBytes(jsonContext))
          {
            var result = ZenFfi.zen_evaluate_expression(expressionPtr, jsonContextPtr);
            return SafeZenResult<Int32>.FromResult(result);
          }
        }
      }
    }).ConfigureAwait(false);

    var data = JsonSerializer.Deserialize<T>(jsonData.Result, JsonOptions.Default);
    return data ?? throw new InvalidOperationException("Deserialization returned null");
  }

  public static async Task<bool> EvaluateUnary(string expression, dynamic context)
  {
    var jsonData = await Task.Run(() =>
    {
      unsafe
      {
        fixed (byte* expressionPtr = Encoding.UTF8.GetBytes(expression))
        {
          string jsonContext = JsonSerializer.Serialize(context, JsonOptions.Default);
          fixed (byte* jsonContextPtr = Encoding.UTF8.GetBytes(jsonContext))
          {
            var result = ZenFfi.zen_evaluate_unary_expression(expressionPtr, jsonContextPtr);
            return SafeZenResult<Int32>.FromResult(result);
          }
        }
      }
    }).ConfigureAwait(false);

    return jsonData.Result == 1;
  }
}
