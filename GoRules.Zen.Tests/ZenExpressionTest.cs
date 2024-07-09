using GoRules.Zen;

namespace GoRules.Zen.Tests;

internal record ExpressionTestCase<T>(String Expression, T Result, dynamic? Context)
{
  internal async Task ValidateExpression()
  {
    var internalResult = await ZenExpression.Evaluate<T>(Expression, Context);
    Assert.That(internalResult, Is.EqualTo(Result), $"Unexpected result for expression: {Expression}");
  }

  internal async Task ValidateUnary()
  {
    if (typeof(T) != typeof(bool))
    {
      throw new InvalidOperationException("ValidateUnary is only valid for TestCase<bool>.");
    }


    bool internalResult = await ZenExpression.EvaluateUnary(Expression, Context ?? new { });
    Assert.That(internalResult, Is.EqualTo((bool)(object)Result!), $"Unexpected result for unary: {Expression}");
  }
}

public class ZenExpressionTest
{
  [Test]
  public async Task TestEvaluateExpression()
  {
    var integerTests = new List<ExpressionTestCase<int>>
    {
      new ExpressionTestCase<int>("1 + 5", 6, null),
      new ExpressionTestCase<int>("10 - 7", 3, null),
      new ExpressionTestCase<int>("a + 5", 10, new { a = 5 }),
      new ExpressionTestCase<int>("a - b", 10, new { a = 20, b = 10 }),
    };

    foreach (var integerTest in integerTests)
    {
      await integerTest.ValidateExpression();
    }

    var stringTests = new List<ExpressionTestCase<string>>
    {
      new ExpressionTestCase<string>("'hello' + ' ' + 'world'", "hello world", null),
      new ExpressionTestCase<string>("'Result is: ' + string(b)", "Result is: 10", new { b = 10 }),
    };

    foreach (var stringTest in stringTests)
    {
      await stringTest.ValidateExpression();
    }
  }

  [Test]
  public async Task TestEvaluateUnary()
  {
    var unaryTests = new List<ExpressionTestCase<bool>>
    {
      new ExpressionTestCase<bool>("> 10", false, new Dictionary<string, object> { { "$", 5 } }),
      new ExpressionTestCase<bool>("> 10", true, new Dictionary<string, object> { { "$", 15 } }),
      new ExpressionTestCase<bool>("'US', 'GB'", true, new Dictionary<string, object> { { "$", "US" } }),
      new ExpressionTestCase<bool>("'US', 'GB'", false, new Dictionary<string, object> { { "$", "AA" } }),
    };

    foreach (var unaryTest in unaryTests)
    {
      await unaryTest.ValidateUnary();
    }
  }
}
