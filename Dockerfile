# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files first to leverage Docker layer caching for restore
COPY src/CSharpApp.Core/CSharpApp.Core.csproj CSharpApp.Core/
COPY src/CSharpApp.Application/CSharpApp.Application.csproj CSharpApp.Application/
COPY src/CSharpApp.Infrastructure/CSharpApp.Infrastructure.csproj CSharpApp.Infrastructure/
COPY src/CSharpApp.Api/CSharpApp.Api.csproj CSharpApp.Api/
RUN dotnet restore CSharpApp.Api/CSharpApp.Api.csproj

# Copy the remaining source and publish
COPY src/CSharpApp.Core/ CSharpApp.Core/
COPY src/CSharpApp.Application/ CSharpApp.Application/
COPY src/CSharpApp.Infrastructure/ CSharpApp.Infrastructure/
COPY src/CSharpApp.Api/ CSharpApp.Api/
RUN dotnet publish CSharpApp.Api/CSharpApp.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

RUN adduser --disabled-password --gecos "" appuser
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CSharpApp.Api.dll"]
