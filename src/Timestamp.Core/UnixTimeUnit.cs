namespace TimestampCode;

/// <summary>
/// Represents the unit of measurement for Unix timestamps.
/// </summary>
public enum UnixTimeUnit
{
    /// <summary>
    /// Seconds since Unix epoch (1970-01-01T00:00:00Z).
    /// </summary>
    Seconds,

    /// <summary>
    /// Milliseconds since Unix epoch (1970-01-01T00:00:00Z).
    /// </summary>
    Milliseconds,

    /// <summary>
    /// Microseconds since Unix epoch (1970-01-01T00:00:00Z).
    /// </summary>
    Microseconds,

    /// <summary>
    /// Nanoseconds since Unix epoch (1970-01-01T00:00:00Z).
    /// Note: Precision may be limited by DateTimeOffset resolution.
    /// </summary>
    Nanoseconds
}
