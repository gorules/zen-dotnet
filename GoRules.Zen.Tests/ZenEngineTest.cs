using System.Text.Json;

namespace GoRules.Zen.Tests;

internal record EngineTestCase(string File, dynamic Input, dynamic Output);

public class ZenEngineTest
{
  private readonly ZenEngineOptions _engineOptions = new ZenEngineOptions
  {
    Loader = path => File.ReadAllBytesAsync(Path.Join("Data", path))
  };

  [Test]
  public async Task TestEngineNew()
  {
    using var engine = new ZenEngine(_engineOptions);
    Assert.That(engine, Is.Not.Null);
  }

  [Test]
  public async Task TestEngineEvaluate()
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
      var response = await engine.Evaluate<JsonDocument>(testCase.File, testCase.Input);

      string received = JsonSerializer.Serialize(response.Result);
      string expected = JsonSerializer.Serialize(testCase.Output);
      Assert.That(received, Is.EqualTo(expected));
      Assert.That(response.Trace, Is.Null);

      var responseWithTrace = await engine.Evaluate<JsonDocument>(testCase.File, testCase.Input, new ZenEvaluationOptions { Trace = true });
      Assert.That(responseWithTrace.Trace, Is.Not.Null);
    }
  }

  [Test]
  public async Task TestEngineGetDecision()
  {
    using var engine = new ZenEngine(_engineOptions);

    var testCases = new List<string> { "table.json", "function.json", "expression.json", "large.json" };

    foreach (var testCase in testCases)
    {
      var decision = await engine.GetDecision(testCase);
      Assert.That(decision, Is.Not.Null);
      Assert.That(decision, Is.TypeOf<ZenDecision>());
    }
  }

  [Test]
  public async Task TestEngineCreateDecision()
  {
    using var engine = new ZenEngine();
    var fileLoader = _engineOptions.Loader!;

    var testCases = new List<string> { "table.json", "function.json", "expression.json", "large.json" };

    foreach (var testCase in testCases)
    {
      var decisionContent = await fileLoader(testCase);
      var decision = engine.CreateDecision(decisionContent);

      Assert.That(decision, Is.Not.Null);
      Assert.That(decision, Is.TypeOf<ZenDecision>());
    }
  }

  [Test]
  public async Task TestEngineEvaluateFailure()
  {
    using var engine = new ZenEngine(_engineOptions);

    Assert.ThrowsAsync<ZenException>(() => engine.Evaluate<dynamic>("function-failure.json", new { input = 10 }));
    Assert.ThrowsAsync<ZenException>(() => engine.Evaluate<dynamic>("not-found.json", new { input = 10 }));
  }

  [Test]
  public async Task TestEngineGetDecisionFailure()
  {
    using var engine = new ZenEngine(_engineOptions);

    Assert.ThrowsAsync<ZenException>(() => engine.GetDecision("not-found.json"));
  }

  [Test]
  public async Task TestEngineCreateDecisionFailure()
  {
    using var engine = new ZenEngine();
    
    Assert.Throws<ZenException>(() => engine.CreateDecision("invalid-json"u8.ToArray()));
  }
}
