FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

RUN addgroup -g 1001 -S appgroup && adduser -u 1001 -S appuser -G appgroup
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY ["services/pos-service/src/PosService.API/PosService.API.csproj", "services/pos-service/src/PosService.API/"]
COPY ["services/pos-service/src/PosService.Application/PosService.Application.csproj", "services/pos-service/src/PosService.Application/"]
COPY ["services/pos-service/src/PosService.Domain/PosService.Domain.csproj", "services/pos-service/src/PosService.Domain/"]
COPY ["services/pos-service/src/PosService.Infrastructure/PosService.Infrastructure.csproj", "services/pos-service/src/PosService.Infrastructure/"]
COPY ["shared/shared-kernel/src/shared-kernel.csproj", "shared/shared-kernel/src/"]
COPY ["shared/shared-infrastructure/src/shared-infrastructure.csproj", "shared/shared-infrastructure/src/"]

RUN dotnet restore "services/pos-service/src/PosService.API/PosService.API.csproj"

COPY . .
WORKDIR "/src/services/pos-service/src/PosService.API"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "PosService.API.dll"]
