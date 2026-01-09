using TimestampCode;
using Xunit;

namespace Timestamp.Core.Tests;

/// <summary>
/// Tests for TimestampCodec Unix time conversion methods.
/// </summary>
public class TimestampCodecTests
{
    private static readonly DateTimeOffset UnixEpoch = new(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

    #region Round-Trip Tests

    [Theory]
    [InlineData(0L, UnixTimeUnit.Seconds)]
    [InlineData(1234567890L, UnixTimeUnit.Seconds)]
    [InlineData(-1234567890L, UnixTimeUnit.Seconds)]
    [InlineData(0L, UnixTimeUnit.Milliseconds)]
    [InlineData(1234567890123L, UnixTimeUnit.Milliseconds)]
    [InlineData(-1234567890123L, UnixTimeUnit.Milliseconds)]
    [InlineData(0L, UnixTimeUnit.Microseconds)]
    [InlineData(1234567890123456L, UnixTimeUnit.Microseconds)]
    [InlineData(-1234567890123456L, UnixTimeUnit.Microseconds)]
    [InlineData(0L, UnixTimeUnit.Nanoseconds)]
    [InlineData(1234567890123456700L, UnixTimeUnit.Nanoseconds)]
    [InlineData(-1234567890123456700L, UnixTimeUnit.Nanoseconds)]
    public void RoundTrip_UnixTime_PreservesValue(long originalValue, UnixTimeUnit unit)
    {
        // Act
        var dateTime = TimestampCodec.FromUnix(originalValue, unit);
        var roundTripped = TimestampCodec.ToUnix(dateTime, unit);

        // Assert
        Assert.Equal(originalValue, roundTripped);
    }

    #endregion

    #region FromUnix Tests

    [Fact]
    public void FromUnix_Epoch_ReturnsEpoch()
    {
        // Act
        var result = TimestampCodec.FromUnix(0, UnixTimeUnit.Seconds);

        // Assert
        Assert.Equal(UnixEpoch, result);
    }

    [Fact]
    public void FromUnix_PositiveSeconds_ReturnsCorrectDateTime()
    {
        // Arrange: 2009-02-13T23:31:30Z
        var expected = new DateTimeOffset(2009, 2, 13, 23, 31, 30, TimeSpan.Zero);

        // Act
        var result = TimestampCodec.FromUnix(1234567890, UnixTimeUnit.Seconds);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void FromUnix_NegativeSeconds_ReturnsCorrectDateTime()
    {
        // Arrange: 1969-12-31T23:59:59Z
        var expected = new DateTimeOffset(1969, 12, 31, 23, 59, 59, TimeSpan.Zero);

        // Act
        var result = TimestampCodec.FromUnix(-1, UnixTimeUnit.Seconds);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void FromUnix_Milliseconds_ReturnsCorrectDateTime()
    {
        // Arrange: 2009-02-13T23:31:30.123Z
        var expected = new DateTimeOffset(2009, 2, 13, 23, 31, 30, 123, TimeSpan.Zero);

        // Act
        var result = TimestampCodec.FromUnix(1234567890123, UnixTimeUnit.Milliseconds);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void FromUnix_Microseconds_ReturnsCorrectDateTime()
    {
        // Arrange: 2009-02-13T23:31:30.123456Z
        var expected = new DateTimeOffset(2009, 2, 13, 23, 31, 30, TimeSpan.Zero)
            .AddTicks(1234560); // 123.456 ms in ticks

        // Act
        var result = TimestampCodec.FromUnix(1234567890123456, UnixTimeUnit.Microseconds);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void FromUnix_Nanoseconds_ReturnsCorrectDateTime()
    {
        // Arrange: Nanoseconds are truncated to 100ns precision (ticks)
        var expected = new DateTimeOffset(2009, 2, 13, 23, 31, 30, TimeSpan.Zero)
            .AddTicks(1234567); // 123.4567 ms in ticks

        // Act
        var result = TimestampCodec.FromUnix(1234567890123456700, UnixTimeUnit.Nanoseconds);

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region ToUnix Tests

    [Fact]
    public void ToUnix_Epoch_ReturnsZero()
    {
        // Act
        var result = TimestampCodec.ToUnix(UnixEpoch, UnixTimeUnit.Seconds);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void ToUnix_UtcDateTime_ReturnsCorrectValue()
    {
        // Arrange
        var dateTime = new DateTimeOffset(2009, 2, 13, 23, 31, 30, TimeSpan.Zero);

        // Act
        var result = TimestampCodec.ToUnix(dateTime, UnixTimeUnit.Seconds);

        // Assert
        Assert.Equal(1234567890, result);
    }

    [Fact]
    public void ToUnix_NonUtcDateTime_NormalizesToUtc()
    {
        // Arrange: Same instant, different offset
        var utcTime = new DateTimeOffset(2009, 2, 13, 23, 31, 30, TimeSpan.Zero);
        var estTime = new DateTimeOffset(2009, 2, 13, 18, 31, 30, TimeSpan.FromHours(-5));

        // Act
        var utcResult = TimestampCodec.ToUnix(utcTime, UnixTimeUnit.Seconds);
        var estResult = TimestampCodec.ToUnix(estTime, UnixTimeUnit.Seconds);

        // Assert
        Assert.Equal(utcResult, estResult);
    }

    [Fact]
    public void ToUnix_Milliseconds_ReturnsCorrectValue()
    {
        // Arrange
        var dateTime = new DateTimeOffset(2009, 2, 13, 23, 31, 30, 123, TimeSpan.Zero);

        // Act
        var result = TimestampCodec.ToUnix(dateTime, UnixTimeUnit.Milliseconds);

        // Assert
        Assert.Equal(1234567890123, result);
    }

    [Fact]
    public void ToUnix_Microseconds_ReturnsCorrectValue()
    {
        // Arrange
        var dateTime = new DateTimeOffset(2009, 2, 13, 23, 31, 30, TimeSpan.Zero)
            .AddTicks(1234560); // 123.456 ms

        // Act
        var result = TimestampCodec.ToUnix(dateTime, UnixTimeUnit.Microseconds);

        // Assert
        Assert.Equal(1234567890123456, result);
    }

    [Fact]
    public void ToUnix_Nanoseconds_ReturnsCorrectValue()
    {
        // Arrange
        var dateTime = new DateTimeOffset(2009, 2, 13, 23, 31, 30, TimeSpan.Zero)
            .AddTicks(1234567); // 123.4567 ms

        // Act
        var result = TimestampCodec.ToUnix(dateTime, UnixTimeUnit.Nanoseconds);

        // Assert
        Assert.Equal(1234567890123456700, result);
    }

    #endregion

    #region ISO8601 Parsing Tests

    [Fact]
    public void ParseIso8601_ValidUtcTimestamp_ReturnsCorrectDateTime()
    {
        // Arrange
        var input = "2009-02-13T23:31:30Z";
        var expected = new DateTimeOffset(2009, 2, 13, 23, 31, 30, TimeSpan.Zero);

        // Act
        var result = TimestampCodec.ParseIso8601(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ParseIso8601_WithFractionalSeconds_ReturnsCorrectDateTime()
    {
        // Arrange
        var input = "2009-02-13T23:31:30.123456Z";
        var expected = new DateTimeOffset(2009, 2, 13, 23, 31, 30, TimeSpan.Zero)
            .AddTicks(1234560);

        // Act
        var result = TimestampCodec.ParseIso8601(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ParseIso8601_WithOffset_NormalizesToUtc()
    {
        // Arrange
        var input = "2009-02-13T18:31:30-05:00";
        var expected = new DateTimeOffset(2009, 2, 13, 23, 31, 30, TimeSpan.Zero);

        // Act
        var result = TimestampCodec.ParseIso8601(input);

        // Assert
        Assert.Equal(expected, result);
        Assert.Equal(TimeSpan.Zero, result.Offset);
    }

    [Fact]
    public void ParseIso8601_NullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => TimestampCodec.ParseIso8601(null!));
    }

    [Fact]
    public void ParseIso8601_EmptyString_ThrowsFormatException()
    {
        // Act & Assert
        Assert.Throws<FormatException>(() => TimestampCodec.ParseIso8601(""));
    }

    [Fact]
    public void ParseIso8601_WhitespaceString_ThrowsFormatException()
    {
        // Act & Assert
        Assert.Throws<FormatException>(() => TimestampCodec.ParseIso8601("   "));
    }

    [Fact]
    public void ParseIso8601_InvalidFormat_ThrowsFormatException()
    {
        // Act & Assert
        Assert.Throws<FormatException>(() => TimestampCodec.ParseIso8601("not a timestamp"));
    }

    #endregion

    #region ISO8601 Formatting Tests

    [Fact]
    public void FormatIso8601_UtcDateTime_ReturnsCorrectFormat()
    {
        // Arrange
        var dateTime = new DateTimeOffset(2009, 2, 13, 23, 31, 30, TimeSpan.Zero);

        // Act
        var result = TimestampCodec.FormatIso8601(dateTime);

        // Assert
        Assert.Equal("2009-02-13T23:31:30.0000000Z", result);
    }

    [Fact]
    public void FormatIso8601_WithFractionalSeconds_IncludesFractionalPart()
    {
        // Arrange
        var dateTime = new DateTimeOffset(2009, 2, 13, 23, 31, 30, 123, TimeSpan.Zero)
            .AddTicks(4560);

        // Act
        var result = TimestampCodec.FormatIso8601(dateTime);

        // Assert
        Assert.Equal("2009-02-13T23:31:30.1234560Z", result);
    }

    [Fact]
    public void FormatIso8601_NonUtcDateTime_NormalizesToUtc()
    {
        // Arrange
        var dateTime = new DateTimeOffset(2009, 2, 13, 18, 31, 30, TimeSpan.FromHours(-5));

        // Act
        var result = TimestampCodec.FormatIso8601(dateTime);

        // Assert
        Assert.Equal("2009-02-13T23:31:30.0000000Z", result);
    }

    [Fact]
    public void FormatIso8601_RoundTrip_PreservesValue()
    {
        // Arrange
        var original = new DateTimeOffset(2009, 2, 13, 23, 31, 30, 123, TimeSpan.Zero);

        // Act
        var formatted = TimestampCodec.FormatIso8601(original);
        var parsed = TimestampCodec.ParseIso8601(formatted);

        // Assert
        Assert.Equal(original, parsed);
    }

    #endregion

    #region Boundary and Overflow Tests

    [Fact]
    public void FromUnix_MaxDateTimeOffset_DoesNotThrow()
    {
        // Arrange: Close to max value
        var maxSeconds = (DateTimeOffset.MaxValue - UnixEpoch).Ticks / 10_000_000;

        // Act & Assert
        var result = TimestampCodec.FromUnix(maxSeconds, UnixTimeUnit.Seconds);
        Assert.True(result > UnixEpoch);
    }

    [Fact]
    public void FromUnix_MinDateTimeOffset_DoesNotThrow()
    {
        // Arrange: Close to min value
        var minSeconds = (DateTimeOffset.MinValue - UnixEpoch).Ticks / 10_000_000;

        // Act & Assert
        var result = TimestampCodec.FromUnix(minSeconds, UnixTimeUnit.Seconds);
        Assert.True(result < UnixEpoch);
    }

    [Fact]
    public void ToUnix_MaxDateTimeOffset_DoesNotThrow()
    {
        // Act & Assert
        var result = TimestampCodec.ToUnix(DateTimeOffset.MaxValue, UnixTimeUnit.Seconds);
        Assert.True(result > 0);
    }

    [Fact]
    public void ToUnix_MinDateTimeOffset_DoesNotThrow()
    {
        // Act & Assert
        var result = TimestampCodec.ToUnix(DateTimeOffset.MinValue, UnixTimeUnit.Seconds);
        Assert.True(result < 0);
    }

    #endregion
}
