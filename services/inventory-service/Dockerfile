FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

RUN addgroup -g 1001 -S appgroup && adduser -u 1001 -S appuser -G appgroup
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY ["services/inventory-service/src/InventoryService.API/InventoryService.API.csproj", "services/inventory-service/src/InventoryService.API/"]
COPY ["services/inventory-service/src/InventoryService.Application/InventoryService.Application.csproj", "services/inventory-service/src/InventoryService.Application/"]
COPY ["services/inventory-service/src/InventoryService.Domain/InventoryService.Domain.csproj", "services/inventory-service/src/InventoryService.Domain/"]
COPY ["services/inventory-service/src/InventoryService.Infrastructure/InventoryService.Infrastructure.csproj", "services/inventory-service/src/InventoryService.Infrastructure/"]
COPY ["shared/shared-kernel/src/shared-kernel.csproj", "shared/shared-kernel/src/"]
COPY ["shared/shared-infrastructure/src/shared-infrastructure.csproj", "shared/shared-infrastructure/src/"]

RUN dotnet restore "services/inventory-service/src/InventoryService.API/InventoryService.API.csproj"

COPY . .
WORKDIR "/src/services/inventory-service/src/InventoryService.API"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "InventoryService.API.dll"]
