using CSharpApp.Core.Common;
using Xunit;

namespace CSharpApp.Tests.Common;

public class ResultTests
{
    [Fact]
    public void Success_ShouldExposeValue_AndNoError()
    {
        var result = Result.Success(42);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Failure_ShouldExposeError_AndThrowWhenAccessingValue()
    {
        var error = Error.NotFound("Product.NotFound", "Not found");
        var result = Result.Failure<int>(error);

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void ImplicitOperator_ShouldWrapValueIntoSuccessResult()
    {
        Result<string> result = "hello";

        Assert.True(result.IsSuccess);
        Assert.Equal("hello", result.Value);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenSuccessResultHasError()
    {
        Assert.Throws<InvalidOperationException>(() => Result.Failure<int>(Error.None));
    }
}
