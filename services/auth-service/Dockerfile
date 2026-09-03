FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# The aspnet:*-alpine base image runs in globalization-invariant mode (no ICU data) to stay
# small; request localization (SharedWeb.PlatformLocalization, en/bn) needs real cultures.
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

RUN addgroup -g 1001 -S appgroup && adduser -u 1001 -S appuser -G appgroup
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY ["services/auth-service/src/AuthService.Api/AuthService.Api.csproj", "services/auth-service/src/AuthService.Api/"]
COPY ["services/auth-service/src/AuthService.Application/AuthService.Application.csproj", "services/auth-service/src/AuthService.Application/"]
COPY ["services/auth-service/src/AuthService.Domain/AuthService.Domain.csproj", "services/auth-service/src/AuthService.Domain/"]
COPY ["services/auth-service/src/AuthService.Infrastructure/AuthService.Infrastructure.csproj", "services/auth-service/src/AuthService.Infrastructure/"]

RUN dotnet restore "services/auth-service/src/AuthService.Api/AuthService.Api.csproj"

COPY . .
WORKDIR "/src/services/auth-service/src/AuthService.Api"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "AuthService.Api.dll"]
