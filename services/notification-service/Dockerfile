FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# The aspnet:*-alpine base image sets DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true
# and ships no ICU data, to keep the image small. This service does real
# culture-aware work (LocalizationMiddleware, ResourceLocalizationService for
# English/Bangla) via CultureInfo.GetCultureInfo, which throws
# CultureNotFoundException in invariant mode on every single request —
# install ICU and turn invariant mode back off.
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

RUN addgroup -g 1001 -S appgroup && adduser -u 1001 -S appuser -G appgroup
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Grpc.Tools ships a glibc-linked protoc binary for linux_arm64/linux_x64 — it
# cannot run on Alpine's musl libc without this compatibility shim (fails with
# a misleading "No such file or directory" from the dynamic loader, not a
# missing-file error). Needed because NotificationService.Api compiles a
# .proto file at build time (see the Protobuf item below).
RUN apk add --no-cache libc6-compat

COPY ["services/notification-service/src/NotificationService.Api/NotificationService.Api.csproj", "services/notification-service/src/NotificationService.Api/"]
COPY ["services/notification-service/src/NotificationService.Application/NotificationService.Application.csproj", "services/notification-service/src/NotificationService.Application/"]
COPY ["services/notification-service/src/NotificationService.Domain/NotificationService.Domain.csproj", "services/notification-service/src/NotificationService.Domain/"]
COPY ["services/notification-service/src/NotificationService.Infrastructure/NotificationService.Infrastructure.csproj", "services/notification-service/src/NotificationService.Infrastructure/"]

RUN dotnet restore "services/notification-service/src/NotificationService.Api/NotificationService.Api.csproj"

COPY . .
WORKDIR "/src/services/notification-service/src/NotificationService.Api"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "NotificationService.Api.dll"]
