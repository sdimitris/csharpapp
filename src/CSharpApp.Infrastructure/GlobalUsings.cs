// Global using directives

global using System.Diagnostics;
global using System.Net;
global using System.Net.Http.Headers;
global using System.Net.Http.Json;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.IdentityModel.Tokens.Jwt;
global using CSharpApp.Core.Common;
global using CSharpApp.Core.Dtos;
global using CSharpApp.Core.Dtos.Requests;
global using CSharpApp.Core.Interfaces;
global using CSharpApp.Core.Settings;
global using CSharpApp.Infrastructure.Configuration;
global using CSharpApp.Infrastructure.Extensions;
global using CSharpApp.Infrastructure.Http;
global using CSharpApp.Infrastructure.Http.Dtos;
global using CSharpApp.Infrastructure.Mappings;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Polly;
global using Polly.CircuitBreaker;
global using Polly.Extensions.Http;
