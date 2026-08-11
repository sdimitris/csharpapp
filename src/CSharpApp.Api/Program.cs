using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

var logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Logging.ClearProviders().AddSerilog(logger);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDefaultConfiguration(builder.Configuration);
builder.Services.AddHttpConfiguration();
builder.Services.AddApplicationServices();
builder.Services.AddProblemDetails();
builder.Services.AddApiVersioning()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "CSharpApp API v1");
        options.RoutePrefix = "swagger";
    });
}

//app.UseHttpsRedirection();

// Outermost middleware: times the whole request (including exception handling and
// endpoint execution) and, because it sits outside UseExceptionHandler below, its
// `finally` block always observes the final response status code — even for requests
// that ended in an unhandled exception.
app.UseRequestPerformanceLogging();

// Converts unhandled exceptions into a ProblemDetails 500 response instead of letting
// them crash the request with no defined status code.
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var problemDetailsService = context.RequestServices.GetRequiredService<IProblemDetailsService>();
        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred while processing the request.",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            }
        });
    });
});

app.UseStatusCodePages();

var versionedEndpointRouteBuilder = app.NewVersionedApi();

versionedEndpointRouteBuilder.MapProductsEndpoints();
versionedEndpointRouteBuilder.MapCategoriesEndpoints();

app.Run();

// Exposed for WebApplicationFactory-based integration tests.
public partial class Program;