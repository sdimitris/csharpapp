using Microsoft.Extensions.Logging;

namespace CSharpApp.Tests.Infrastructure.Support;

/// <summary>
/// Minimal <see cref="ILogger{TCategoryName}"/> fake that records every logged entry's
/// level and formatted message, so tests can assert on what a component logged without
/// wiring up a full logging provider.
/// </summary>
public sealed class CapturingLogger<T> : ILogger<T>
{
    public List<(LogLevel Level, string Message)> Entries { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Entries.Add((logLevel, formatter(state, exception)));
    }
}
