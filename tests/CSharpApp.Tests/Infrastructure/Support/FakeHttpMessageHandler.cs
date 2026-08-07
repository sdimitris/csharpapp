using System.Net;
using System.Net.Http.Json;

namespace CSharpApp.Tests.Infrastructure.Support;

/// <summary>
/// Minimal fake <see cref="HttpMessageHandler"/> that returns a canned response (or invokes
/// a responder callback) so HTTP-dependent components can be tested without any network I/O.
/// </summary>
public sealed class FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
{
    public List<HttpRequestMessage> Requests { get; } = [];

    public static FakeHttpMessageHandler ReturningJson<T>(T payload, HttpStatusCode statusCode = HttpStatusCode.OK)
        => new(_ => new HttpResponseMessage(statusCode) { Content = JsonContent.Create(payload) });

    public static FakeHttpMessageHandler ReturningStatus(HttpStatusCode statusCode, string? body = null)
        => new(_ => new HttpResponseMessage(statusCode) { Content = new StringContent(body ?? string.Empty) });

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);
        return Task.FromResult(responder(request));
    }
}
