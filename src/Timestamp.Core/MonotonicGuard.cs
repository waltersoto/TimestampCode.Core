namespace TimestampCode;

/// <summary>
/// Detects backward jumps in observed timestamps.
/// Useful for validating time sources or incoming event streams.
/// This class does not apply any correction or smoothing.
/// </summary>
public class MonotonicGuard
{
    private DateTimeOffset? _lastSeen;

    /// <summary>
    /// Gets the last observed timestamp, or null if no timestamps have been checked yet.
    /// </summary>
    public DateTimeOffset? LastSeen => _lastSeen;

    /// <summary>
    /// Evaluates the current timestamp against the previously observed one.
    /// </summary>
    /// <param name="current">The current timestamp to check.</param>
    /// <returns>A MonotonicResult indicating whether a backward jump occurred.</returns>
    public MonotonicResult Check(DateTimeOffset current)
    {
        var previous = _lastSeen;
        
        if (previous == null)
        {
            // First observation
            _lastSeen = current;
            return new MonotonicResult(
                IsBackwardJump: false,
                Delta: TimeSpan.Zero,
                Previous: null,
                Current: current);
        }

        var delta = current - previous.Value;
        var isBackwardJump = current < previous.Value;

        // Update last seen
        _lastSeen = current;

        return new MonotonicResult(
            IsBackwardJump: isBackwardJump,
            Delta: delta,
            Previous: previous,
            Current: current);
    }

    /// <summary>
    /// Resets the guard, clearing the last observed timestamp.
    /// </summary>
    public void Reset()
    {
        _lastSeen = null;
    }
}
