namespace TimestampCode;

/// <summary>
/// Represents the result of a monotonic timestamp check.
/// </summary>
/// <param name="IsBackwardJump">True if the current timestamp is earlier than the previous one.</param>
/// <param name="Delta">The time difference between current and previous timestamps. Negative if backward jump.</param>
/// <param name="Previous">The previous timestamp, or null if this is the first observation.</param>
/// <param name="Current">The current timestamp being evaluated.</param>
public record MonotonicResult(
    bool IsBackwardJump,
    TimeSpan Delta,
    DateTimeOffset? Previous,
    DateTimeOffset Current);
