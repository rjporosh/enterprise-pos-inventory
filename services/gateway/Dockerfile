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

COPY ["services/gateway/src/Gateway.Api/Gateway.Api.csproj", "services/gateway/src/Gateway.Api/"]

RUN dotnet restore "services/gateway/src/Gateway.Api/Gateway.Api.csproj"

COPY . .
WORKDIR "/src/services/gateway/src/Gateway.Api"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Gateway.Api.dll"]
