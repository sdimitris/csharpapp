namespace CSharpApp.Core.Settings;

public sealed class HttpClientSettings
{
    /// <summary>Handler lifetime, in minutes, before <see cref="System.Net.Http.IHttpClientFactory"/> recycles it.</summary>
    public int LifeTime { get; set; } = 10;

    /// <summary>Number of retry attempts for transient failures.</summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>Base sleep duration, in milliseconds, used for the exponential backoff between retries.</summary>
    public int SleepDuration { get; set; } = 200;

    /// <summary>Number of consecutive failures before the circuit breaker opens.</summary>
    public int CircuitBreakerFailureThreshold { get; set; } = 5;

    /// <summary>Duration, in seconds, the circuit stays open before allowing a trial request.</summary>
    public int CircuitBreakerDurationOfBreakSeconds { get; set; } = 30;

    /// <summary>Per-request timeout, in seconds.</summary>
    public int TimeoutSeconds { get; set; } = 15;
}