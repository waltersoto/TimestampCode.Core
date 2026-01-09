using System.Globalization;

namespace TimestampCode;

/// <summary>
/// Provides strict, deterministic conversion and formatting of timestamps.
/// All operations normalize to UTC and enforce explicit unit handling.
/// </summary>
public static class TimestampCodec
{
    private static readonly DateTimeOffset UnixEpoch = new(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

    // Conversion factors relative to seconds
    private const long MillisecondsPerSecond = 1_000L;
    private const long MicrosecondsPerSecond = 1_000_000L;
    private const long NanosecondsPerSecond = 1_000_000_000L;

    // Ticks per unit (100ns = 1 tick in .NET)
    private const long TicksPerMicrosecond = 10L;
    private const long TicksPerMillisecond = 10_000L;
    private const long TicksPerSecond = 10_000_000L;

    /// <summary>
    /// Converts a Unix timestamp in the specified unit to a UTC DateTimeOffset.
    /// </summary>
    /// <param name="value">The Unix timestamp value.</param>
    /// <param name="unit">The unit of the timestamp.</param>
    /// <returns>A DateTimeOffset normalized to UTC.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the conversion would overflow DateTimeOffset limits.</exception>
    public static DateTimeOffset FromUnix(long value, UnixTimeUnit unit)
    {
        try
        {
            return unit switch
            {
                UnixTimeUnit.Seconds => UnixEpoch.AddSeconds(value),
                UnixTimeUnit.Milliseconds => UnixEpoch.AddMilliseconds(value),
                UnixTimeUnit.Microseconds => UnixEpoch.AddTicks(checked(value * TicksPerMicrosecond)),
                UnixTimeUnit.Nanoseconds => UnixEpoch.AddTicks(value / 100), // 100ns per tick
                _ => throw new ArgumentException($"Unknown unit: {unit}", nameof(unit))
            };
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                $"Unix timestamp {value} {unit} is outside the valid DateTimeOffset range.");
        }
    }

    /// <summary>
    /// Converts a DateTimeOffset to a Unix timestamp in the specified unit.
    /// The input timestamp is normalized to UTC before conversion.
    /// </summary>
    /// <param name="timestamp">The timestamp to convert.</param>
    /// <param name="unit">The desired output unit.</param>
    /// <returns>A Unix timestamp in the specified unit.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the conversion would overflow long limits.</exception>
    public static long ToUnix(DateTimeOffset timestamp, UnixTimeUnit unit)
    {
        // Normalize to UTC
        var utcTimestamp = timestamp.ToUniversalTime();
        var elapsed = utcTimestamp - UnixEpoch;

        try
        {
            return unit switch
            {
                UnixTimeUnit.Seconds => checked((long)elapsed.TotalSeconds),
                UnixTimeUnit.Milliseconds => checked((long)elapsed.TotalMilliseconds),
                UnixTimeUnit.Microseconds => checked(elapsed.Ticks / TicksPerMicrosecond),
                UnixTimeUnit.Nanoseconds => checked(elapsed.Ticks * 100), // 100ns per tick
                _ => throw new ArgumentException($"Unknown unit: {unit}", nameof(unit))
            };
        }
        catch (OverflowException)
        {
            throw new ArgumentOutOfRangeException(
                nameof(timestamp),
                $"Timestamp {timestamp} cannot be represented as Unix {unit} within long range.");
        }
    }

    /// <summary>
    /// Parses an RFC3339/ISO8601 timestamp string and normalizes it to UTC.
    /// </summary>
    /// <param name="value">The timestamp string to parse.</param>
    /// <returns>A DateTimeOffset normalized to UTC.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    /// <exception cref="FormatException">Thrown when the input is not a valid ISO8601/RFC3339 timestamp.</exception>
    public static DateTimeOffset ParseIso8601(string value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        if (string.IsNullOrWhiteSpace(value))
            throw new FormatException("Timestamp string cannot be empty or whitespace.");

        try
        {
            // Parse with strict ISO8601/RFC3339 format
            var parsed = DateTimeOffset.Parse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind);

            // Normalize to UTC
            return parsed.ToUniversalTime();
        }
        catch (FormatException)
        {
            throw new FormatException($"Invalid ISO8601/RFC3339 timestamp: '{value}'");
        }
    }

    /// <summary>
    /// Formats a DateTimeOffset as an RFC3339/ISO8601 timestamp with UTC 'Z' suffix.
    /// The input is normalized to UTC before formatting.
    /// </summary>
    /// <param name="timestamp">The timestamp to format.</param>
    /// <returns>An RFC3339/ISO8601 formatted string with 'Z' suffix.</returns>
    public static string FormatIso8601(DateTimeOffset timestamp)
    {
        // Normalize to UTC and format with 'Z' suffix
        return timestamp.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture);
    }
}
