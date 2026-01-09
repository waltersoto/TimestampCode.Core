using TimestampCode;
using Xunit;

namespace Timestamp.Core.Tests;

/// <summary>
/// Tests for MonotonicGuard backward jump detection.
/// </summary>
public class MonotonicGuardTests
{
    [Fact]
    public void Check_FirstObservation_ReturnsNoBackwardJump()
    {
        // Arrange
        var guard = new MonotonicGuard();
        var timestamp = DateTimeOffset.UtcNow;

        // Act
        var result = guard.Check(timestamp);

        // Assert
        Assert.False(result.IsBackwardJump);
        Assert.Equal(TimeSpan.Zero, result.Delta);
        Assert.Null(result.Previous);
        Assert.Equal(timestamp, result.Current);
    }

    [Fact]
    public void Check_ForwardProgress_ReturnsNoBackwardJump()
    {
        // Arrange
        var guard = new MonotonicGuard();
        var first = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var second = new DateTimeOffset(2024, 1, 1, 12, 0, 1, TimeSpan.Zero);

        // Act
        guard.Check(first);
        var result = guard.Check(second);

        // Assert
        Assert.False(result.IsBackwardJump);
        Assert.Equal(TimeSpan.FromSeconds(1), result.Delta);
        Assert.Equal(first, result.Previous);
        Assert.Equal(second, result.Current);
    }

    [Fact]
    public void Check_BackwardJump_ReturnsBackwardJump()
    {
        // Arrange
        var guard = new MonotonicGuard();
        var first = new DateTimeOffset(2024, 1, 1, 12, 0, 1, TimeSpan.Zero);
        var second = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);

        // Act
        guard.Check(first);
        var result = guard.Check(second);

        // Assert
        Assert.True(result.IsBackwardJump);
        Assert.Equal(TimeSpan.FromSeconds(-1), result.Delta);
        Assert.Equal(first, result.Previous);
        Assert.Equal(second, result.Current);
    }

    [Fact]
    public void Check_IdenticalTimestamps_ReturnsNoBackwardJump()
    {
        // Arrange
        var guard = new MonotonicGuard();
        var timestamp = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);

        // Act
        guard.Check(timestamp);
        var result = guard.Check(timestamp);

        // Assert
        Assert.False(result.IsBackwardJump);
        Assert.Equal(TimeSpan.Zero, result.Delta);
        Assert.Equal(timestamp, result.Previous);
        Assert.Equal(timestamp, result.Current);
    }

    [Fact]
    public void Check_MultipleObservations_TracksCorrectly()
    {
        // Arrange
        var guard = new MonotonicGuard();
        var t1 = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var t2 = new DateTimeOffset(2024, 1, 1, 12, 0, 1, TimeSpan.Zero);
        var t3 = new DateTimeOffset(2024, 1, 1, 12, 0, 2, TimeSpan.Zero);
        var t4 = new DateTimeOffset(2024, 1, 1, 12, 0, 1, TimeSpan.Zero); // Backward

        // Act
        var r1 = guard.Check(t1);
        var r2 = guard.Check(t2);
        var r3 = guard.Check(t3);
        var r4 = guard.Check(t4);

        // Assert
        Assert.False(r1.IsBackwardJump);
        Assert.False(r2.IsBackwardJump);
        Assert.False(r3.IsBackwardJump);
        Assert.True(r4.IsBackwardJump);
        Assert.Equal(TimeSpan.FromSeconds(-1), r4.Delta);
    }

    [Fact]
    public void Check_AfterBackwardJump_UpdatesLastSeen()
    {
        // Arrange
        var guard = new MonotonicGuard();
        var t1 = new DateTimeOffset(2024, 1, 1, 12, 0, 2, TimeSpan.Zero);
        var t2 = new DateTimeOffset(2024, 1, 1, 12, 0, 1, TimeSpan.Zero); // Backward
        var t3 = new DateTimeOffset(2024, 1, 1, 12, 0, 3, TimeSpan.Zero);

        // Act
        guard.Check(t1);
        guard.Check(t2);
        var result = guard.Check(t3);

        // Assert
        Assert.False(result.IsBackwardJump);
        Assert.Equal(t2, result.Previous); // Previous should be t2, not t1
        Assert.Equal(TimeSpan.FromSeconds(2), result.Delta);
    }

    [Fact]
    public void Reset_ClearsLastSeen()
    {
        // Arrange
        var guard = new MonotonicGuard();
        var timestamp = DateTimeOffset.UtcNow;
        guard.Check(timestamp);

        // Act
        guard.Reset();

        // Assert
        Assert.Null(guard.LastSeen);
    }

    [Fact]
    public void Reset_NextCheckIsFirstObservation()
    {
        // Arrange
        var guard = new MonotonicGuard();
        var t1 = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var t2 = new DateTimeOffset(2024, 1, 1, 12, 0, 1, TimeSpan.Zero);
        guard.Check(t1);

        // Act
        guard.Reset();
        var result = guard.Check(t2);

        // Assert
        Assert.False(result.IsBackwardJump);
        Assert.Equal(TimeSpan.Zero, result.Delta);
        Assert.Null(result.Previous);
    }

    [Fact]
    public void LastSeen_ReflectsLastCheckedTimestamp()
    {
        // Arrange
        var guard = new MonotonicGuard();
        var t1 = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var t2 = new DateTimeOffset(2024, 1, 1, 12, 0, 1, TimeSpan.Zero);

        // Act
        Assert.Null(guard.LastSeen);
        guard.Check(t1);
        Assert.Equal(t1, guard.LastSeen);
        guard.Check(t2);
        Assert.Equal(t2, guard.LastSeen);
    }

    [Fact]
    public void Check_WithDifferentOffsets_ComparesCorrectly()
    {
        // Arrange
        var guard = new MonotonicGuard();
        var utc = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
        var est = new DateTimeOffset(2024, 1, 1, 7, 0, 0, TimeSpan.FromHours(-5)); // Same instant

        // Act
        guard.Check(utc);
        var result = guard.Check(est);

        // Assert
        Assert.False(result.IsBackwardJump);
        Assert.Equal(TimeSpan.Zero, result.Delta);
    }
}
