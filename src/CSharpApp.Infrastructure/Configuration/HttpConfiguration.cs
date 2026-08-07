namespace CSharpApp.Infrastructure.Configuration;

public static class HttpConfiguration
{
    public static IServiceCollection AddHttpConfiguration(this IServiceCollection services)
    {
        services.AddTransient<AuthenticationDelegatingHandler>();

        // Dedicated client for the login endpoint: must not go through the auth handler,
        // otherwise obtaining a token would recursively require a token.
        services.AddHttpClient(HttpClientNames.Auth, ConfigureBaseClient)
            .AddPolicyHandler(GetRetryPolicy)
            .AddPolicyHandler(GetCircuitBreakerPolicy);

        services.AddHttpClient<IProductsApiClient, ProductsApiClient>(HttpClientNames.Products, ConfigureBaseClient)
            .AddHttpMessageHandler<AuthenticationDelegatingHandler>()
            .AddPolicyHandler(GetRetryPolicy)
            .AddPolicyHandler(GetCircuitBreakerPolicy);

        services.AddHttpClient<ICategoriesApiClient, CategoriesApiClient>(HttpClientNames.Categories, ConfigureBaseClient)
            .AddHttpMessageHandler<AuthenticationDelegatingHandler>()
            .AddPolicyHandler(GetRetryPolicy)
            .AddPolicyHandler(GetCircuitBreakerPolicy);

        return services;
    }

    private static void ConfigureBaseClient(IServiceProvider provider, HttpClient client)
    {
        var restApiSettings = provider.GetRequiredService<IOptions<RestApiSettings>>().Value;
        var httpClientSettings = provider.GetRequiredService<IOptions<HttpClientSettings>>().Value;

        client.BaseAddress = new Uri(restApiSettings.BaseUrl!);
        client.Timeout = TimeSpan.FromSeconds(httpClientSettings.TimeoutSeconds);
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(IServiceProvider provider, HttpRequestMessage _)
    {
        var settings = provider.GetRequiredService<IOptions<HttpClientSettings>>().Value;
        var logger = provider.GetRequiredService<ILogger<AuthenticationDelegatingHandler>>();

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                settings.RetryCount,
                retryAttempt => TimeSpan.FromMilliseconds(settings.SleepDuration * Math.Pow(2, retryAttempt - 1)),
                onRetry: (outcome, delay, attempt, _) =>
                    logger.LogWarning(
                        "Retry {Attempt} for third-party request after {Delay}ms due to {Reason}.",
                        attempt, delay.TotalMilliseconds, outcome.Result?.StatusCode.ToString() ?? outcome.Exception?.Message));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(IServiceProvider provider, HttpRequestMessage _)
    {
        var settings = provider.GetRequiredService<IOptions<HttpClientSettings>>().Value;

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                settings.CircuitBreakerFailureThreshold,
                TimeSpan.FromSeconds(settings.CircuitBreakerDurationOfBreakSeconds));
    }
}