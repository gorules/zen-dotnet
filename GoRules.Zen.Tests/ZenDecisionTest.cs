using System.Text.Json;

namespace GoRules.Zen.Tests;

public class ZenDecisionTest
{
  private readonly ZenEngineOptions _engineOptions = new ZenEngineOptions
  {
    Loader = path => File.ReadAllBytesAsync(Path.Join("Data", path))
  };

  [Test]
  public async Task TestDecisionEvaluate()
  {
    using var engine = new ZenEngine(_engineOptions);

    var testCases = new List<EngineTestCase>()
    {
      new EngineTestCase("table.json", new { input = 5 }, new { output = 0 }),
      new EngineTestCase("table.json", new { input = 15 }, new { output = 10 }),
      new EngineTestCase("function.json", new { input = 1 }, new { output = 2 }),
      new EngineTestCase("function.json", new { input = 5 }, new { output = 10 }),
      new EngineTestCase("function.json", new { input = 15 }, new { output = 30 }),
      new EngineTestCase(
        "expression.json",
        new { numbers = new[] { 1, 5, 15, 25 }, firstName = "John", lastName = "Doe" },
        new { deep = new { nested = new { sum = 46 } }, fullName = "John Doe", largeNumbers = new[] { 15, 25 }, smallNumbers = new[] { 1, 5 } }
      )
    };

    foreach (var testCase in testCases)
    {
      using var decision = await engine.GetDecision(testCase.File);
      var response = await decision.Evaluate<JsonDocument>(testCase.Input);

      string received = JsonSerializer.Serialize(response.Result);
      string expected = JsonSerializer.Serialize(testCase.Output);
      Assert.That(received, Is.EqualTo(expected));
      Assert.That(response.Trace, Is.Null);
      
      var responseWithTrace = await decision.Evaluate<JsonDocument>(testCase.Input, new ZenEvaluationOptions { Trace = true });
      Assert.That(responseWithTrace.Trace, Is.Not.Null);
    }
  }

  [Test]
  public async Task TestDecisionEvaluateFailure()
  {
    using var engine = new ZenEngine(_engineOptions);

    Assert.ThrowsAsync<ZenException>(async () =>
    {
      using var decision = await engine.GetDecision("function-failure.json");
      await decision.Evaluate<dynamic>(new { input = 10 });
    });

    Assert.ThrowsAsync<ZenException>(() => engine.GetDecision("not-found.json"));
  }
}
