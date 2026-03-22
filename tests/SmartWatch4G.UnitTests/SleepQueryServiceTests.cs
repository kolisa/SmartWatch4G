using System;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SmartWatch4G.Domain.Common;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;
using SmartWatch4G.Domain.Interfaces.Services;
using SmartWatch4G.Infrastructure.Services;
using Xunit;

namespace SmartWatch4G.UnitTests;

public sealed class SleepQueryServiceTests
{
    private readonly Mock<ISleepDataRepository> _sleepRepo = new();
    private readonly Mock<IRriDataRepository> _rriRepo = new();
    private readonly Mock<IWownAlgoClient> _algoClient = new();
    private readonly SleepQueryService _sut;

    public SleepQueryServiceTests()
    {
        _sut = new SleepQueryService(
            _sleepRepo.Object,
            _rriRepo.Object,
            _algoClient.Object,
            NullLogger<SleepQueryService>.Instance);
    }

    // ── GetSleepResultAsync ────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("bad-date")]
    [InlineData("20240115")]
    public async Task GetSleepResultAsync_InvalidDate_ReturnsFailure(string date)
    {
        ServiceResult<SleepResult?> result =
            await _sut.GetSleepResultAsync("DEVICE1", date);

        Assert.True(result.IsFailure);
        Assert.Equal(400, result.ErrorCode);
    }

    [Fact]
    public async Task GetSleepResultAsync_NoData_ReturnsSuccessWithNull()
    {
        _sleepRepo
            .Setup(r => r.GetByDeviceAndDateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        ServiceResult<SleepResult?> result =
            await _sut.GetSleepResultAsync("DEVICE1", "2024-01-15");

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetSleepResultAsync_AlgoReturnsNull_ReturnsFailure()
    {
        var slot = new SleepDataRecord { SleepJson = "{}" };

        _sleepRepo
            .Setup(r => r.GetByDeviceAndDateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([slot]);

        _rriRepo
            .Setup(r => r.GetByDeviceAndDateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _algoClient
            .Setup(c => c.CalculateSleepAsync(It.IsAny<SleepCalcRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SleepCalcResult?)null);

        ServiceResult<SleepResult?> result =
            await _sut.GetSleepResultAsync("DEVICE1", "2024-01-15");

        Assert.True(result.IsFailure);
        Assert.Equal(502, result.ErrorCode);
    }

    [Fact]
    public async Task GetSleepResultAsync_AlgoSuccess_ReturnsMappedResult()
    {
        var slot = new SleepDataRecord { SleepJson = "{}" };

        _sleepRepo
            .Setup(r => r.GetByDeviceAndDateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([slot]);

        _rriRepo
            .Setup(r => r.GetByDeviceAndDateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var calcResult = new SleepCalcResult
        {
            StartTime = "2024-01-14 22:00:00",
            EndTime   = "2024-01-15 06:00:00",
            HeartRate = 58,
            Sections  =
            [
                new SleepSection { Type = 3, Start = "2024-01-14 22:00:00", End = "2024-01-14 23:00:00" },
                new SleepSection { Type = 4, Start = "2024-01-14 23:00:00", End = "2024-01-15 06:00:00" }
            ]
        };

        _algoClient
            .Setup(c => c.CalculateSleepAsync(It.IsAny<SleepCalcRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(calcResult);

        ServiceResult<SleepResult?> result =
            await _sut.GetSleepResultAsync("DEVICE1", "2024-01-15");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("DEVICE1", result.Value!.DeviceId);
        Assert.Equal("2024-01-15", result.Value.SleepDate);
        Assert.Equal(60, result.Value.DeepSleepMinutes);
        Assert.Equal(420, result.Value.LightSleepMinutes);
        Assert.Equal(58, result.Value.SleepHeartRate);
    }

    // ── GetSleepResultsByDateRangeAsync ────────────────────────────────────

    [Theory]
    [InlineData("bad", "2024-01-15")]
    [InlineData("2024-01-10", "bad")]
    [InlineData(null, "2024-01-15")]
    public async Task GetSleepResultsByDateRangeAsync_InvalidDates_ReturnsFailure(
        string? from, string? to)
    {
        ServiceResult<IReadOnlyList<SleepResult>> result =
            await _sut.GetSleepResultsByDateRangeAsync("DEVICE1", from!, to!);

        Assert.True(result.IsFailure);
        Assert.Equal(400, result.ErrorCode);
    }

    [Fact]
    public async Task GetSleepResultsByDateRangeAsync_NoData_ReturnsEmptyList()
    {
        _sleepRepo
            .Setup(r => r.GetByDeviceAndDateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        ServiceResult<IReadOnlyList<SleepResult>> result =
            await _sut.GetSleepResultsByDateRangeAsync("DEVICE1", "2024-01-10", "2024-01-12");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value!);
    }
}
