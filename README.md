# TimestampCode.Core

A strict, lightweight .NET library for converting, parsing, and validating timestamps across common wire formats. Designed to eliminate repeated time-handling bugs by enforcing UTC normalization, explicit units, and deterministic behavior.

## Features

- **Reliable Unix Time Conversion**: Convert between Unix time units (seconds, milliseconds, microseconds, nanoseconds) and `DateTimeOffset`
- **ISO8601/RFC3339 Support**: Parse and format timestamps with strict UTC normalization
- **Backward Jump Detection**: Detect time anomalies in event streams or time sources
- **Zero Dependencies**: No external dependencies beyond .NET 10.0
- **Deterministic**: Same inputs always produce same outputs
- **UTC-Only**: All operations normalize to UTC for consistency

## Installation

```bash
dotnet add package TimestampCode.Core
```

## Quick Start

### Unix Time Conversion

```csharp
using TimestampCode;

// Convert Unix timestamp to DateTimeOffset
var dateTime = TimestampCodec.FromUnix(1234567890, UnixTimeUnit.Seconds);
// Result: 2009-02-13T23:31:30Z

// Convert DateTimeOffset to Unix timestamp
var unixTime = TimestampCodec.ToUnix(dateTime, UnixTimeUnit.Milliseconds);
// Result: 1234567890000

// Works with all units
var microseconds = TimestampCodec.ToUnix(dateTime, UnixTimeUnit.Microseconds);
var nanoseconds = TimestampCodec.ToUnix(dateTime, UnixTimeUnit.Nanoseconds);
```

### ISO8601/RFC3339 Parsing and Formatting

```csharp
// Parse ISO8601 timestamp (automatically normalizes to UTC)
var parsed = TimestampCodec.ParseIso8601("2009-02-13T18:31:30-05:00");
// Result: 2009-02-13T23:31:30Z (normalized to UTC)

// Format as ISO8601 with UTC 'Z' suffix
var formatted = TimestampCodec.FormatIso8601(parsed);
// Result: "2009-02-13T23:31:30.0000000Z"
```

### Backward Jump Detection

```csharp
var guard = new MonotonicGuard();

// First observation
var result1 = guard.Check(DateTimeOffset.UtcNow);
// IsBackwardJump: false, Delta: 0

// Forward progress
var result2 = guard.Check(DateTimeOffset.UtcNow.AddSeconds(1));
// IsBackwardJump: false, Delta: 1 second

// Backward jump detected!
var result3 = guard.Check(DateTimeOffset.UtcNow.AddSeconds(-1));
// IsBackwardJump: true, Delta: negative

// Reset when needed
guard.Reset();
```

## API Reference

### UnixTimeUnit Enum

Defines the unit of measurement for Unix timestamps:

- `Seconds` - Seconds since Unix epoch (1970-01-01T00:00:00Z)
- `Milliseconds` - Milliseconds since Unix epoch
- `Microseconds` - Microseconds since Unix epoch
- `Nanoseconds` - Nanoseconds since Unix epoch (100ns precision)

### TimestampCodec Class

Static class providing timestamp conversion and formatting methods.

#### FromUnix

```csharp
DateTimeOffset FromUnix(long value, UnixTimeUnit unit)
```

Converts a Unix timestamp in the given unit to a UTC `DateTimeOffset`.

**Throws**: `ArgumentOutOfRangeException` if the value is outside valid `DateTimeOffset` range.

#### ToUnix

```csharp
long ToUnix(DateTimeOffset timestamp, UnixTimeUnit unit)
```

Converts a `DateTimeOffset` to a Unix timestamp in the given unit. Input is normalized to UTC.

**Throws**: `ArgumentOutOfRangeException` if the timestamp cannot be represented within `long` range for the given unit.

#### ParseIso8601

```csharp
DateTimeOffset ParseIso8601(string value)
```

Parses an RFC3339/ISO8601 timestamp and normalizes it to UTC.

**Throws**: 
- `ArgumentNullException` if value is null
- `FormatException` if the input is not a valid ISO8601/RFC3339 timestamp

#### FormatIso8601

```csharp
string FormatIso8601(DateTimeOffset timestamp)
```

Formats a `DateTimeOffset` as RFC3339/ISO8601 with UTC 'Z' suffix. Input is normalized to UTC.

### MonotonicGuard Class

Detects backward jumps in observed timestamps.

#### Properties

- `LastSeen` - Gets the last observed timestamp, or null if none have been checked

#### Methods

##### Check

```csharp
MonotonicResult Check(DateTimeOffset current)
```

Evaluates the current timestamp against the previously observed one.

##### Reset

```csharp
void Reset()
```

Resets the guard, clearing the last observed timestamp.

### MonotonicResult Record

Represents the result of a monotonic timestamp check.

**Properties**:
- `IsBackwardJump` (bool) - True if current timestamp is earlier than previous
- `Delta` (TimeSpan) - Time difference between current and previous timestamps
- `Previous` (DateTimeOffset?) - Previous timestamp, or null for first observation
- `Current` (DateTimeOffset) - Current timestamp being evaluated

## Design Principles

- **Small surface area, high correctness**: Focused API with strict validation
- **Explicit units and UTC-only semantics**: No ambiguity in time representation
- **No IO, no logging, no background threads**: Pure computation only
- **Deterministic and testable**: Same inputs always produce same outputs
- **Zero external dependencies**: Only depends on .NET BCL

## UTC Handling

All operations enforce UTC semantics:

- Parsing results are always normalized to UTC
- Formatting always emits UTC with 'Z' suffix
- Non-UTC inputs are accepted but automatically converted
- Unix epoch is always 1970-01-01T00:00:00Z

## Overflow and Boundary Handling

The library performs overflow checking on all conversions:

- `FromUnix` validates that the Unix timestamp can be represented as `DateTimeOffset`
- `ToUnix` validates that the `DateTimeOffset` can be represented as `long` for the given unit
- Nanosecond precision uses `long` arithmetic with 100ns resolution (tick-level)

## Non-Goals

This library intentionally does NOT:

- Replace system clocks
- Provide high-resolution timers
- Act as a time synchronization library
- Manage leap seconds beyond standard `DateTimeOffset` behavior
- Include scheduling or timer functionality
- Support time zones beyond UTC

## Testing

The library includes comprehensive tests covering:

- Round-trip conversions for all Unix units
- ISO8601 parsing edge cases (offsets, fractional seconds)
- UTC normalization
- Backward jump detection
- Overflow and boundary conditions

Run tests:

```bash
dotnet test
```

## License

MIT License - see LICENSE file for details

## Contributing

This library follows a conservative API-first approach. Changes should maintain strict backward compatibility and deterministic behavior.
