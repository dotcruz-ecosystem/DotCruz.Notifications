FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
USER root
RUN apk add --no-cache icu-libs icu-data-full
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/DotCruz.Notifications.Api/DotCruz.Notifications.Api.csproj", "src/DotCruz.Notifications.Api/"]
COPY ["src/DotCruz.Notifications.Application/DotCruz.Notifications.Application.csproj", "src/DotCruz.Notifications.Application/"]
COPY ["src/DotCruz.Notifications.Domain/DotCruz.Notifications.Domain.csproj", "src/DotCruz.Notifications.Domain/"]
COPY ["src/DotCruz.Notifications.Infrastructure/DotCruz.Notifications.Infrastructure.csproj", "src/DotCruz.Notifications.Infrastructure/"]
COPY ["src/DotCruz.Notifications.CrossCutting/DotCruz.Notifications.CrossCutting.csproj", "src/DotCruz.Notifications.CrossCutting/"]
RUN dotnet restore "./src/DotCruz.Notifications.Api/DotCruz.Notifications.Api.csproj"
COPY . .
WORKDIR "/src/src/DotCruz.Notifications.Api"
RUN dotnet build "./DotCruz.Notifications.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DotCruz.Notifications.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DotCruz.Notifications.Api.dll"]
