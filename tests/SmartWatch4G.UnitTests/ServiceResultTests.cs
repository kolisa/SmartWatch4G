using SmartWatch4G.Domain.Common;
using Xunit;

namespace SmartWatch4G.UnitTests;

public sealed class ServiceResultTests
{
    [Fact]
    public void Ok_SetsIsSuccessTrue_AndStoresValue()
    {
        ServiceResult<int> result = ServiceResult<int>.Ok(42);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
        Assert.Null(result.Error);
        Assert.Equal(0, result.ErrorCode);
    }

    [Fact]
    public void Ok_WithNullableReferenceValue_IsSuccess()
    {
        ServiceResult<string?> result = ServiceResult<string?>.Ok(null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public void Fail_SetsIsSuccessFalse_AndStoresError()
    {
        ServiceResult<int> result = ServiceResult<int>.Fail("Something went wrong", 400);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Something went wrong", result.Error);
        Assert.Equal(400, result.ErrorCode);
        Assert.Equal(default, result.Value);
    }

    [Fact]
    public void Fail_WithoutErrorCode_DefaultsToZero()
    {
        ServiceResult<string> result = ServiceResult<string>.Fail("oops");

        Assert.Equal(0, result.ErrorCode);
        Assert.Equal("oops", result.Error);
    }

    [Fact]
    public void Ok_WithComplexType_StoresCorrectly()
    {
        var list = new List<string> { "a", "b" };
        ServiceResult<IReadOnlyList<string>> result = ServiceResult<IReadOnlyList<string>>.Ok(list);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
    }
}
