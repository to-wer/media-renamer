FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

RUN apt-get update && apt-get install -y \
    libzen0t64 \
    libmediainfo0v5 \
    libmms0 \
    openssl \
    zlib1g \
    libcurl4 \
    libglib2.0-0 \
    && rm -rf /var/lib/apt/lists/*

USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/MediaRenamer.Api/MediaRenamer.Api.csproj", "src/MediaRenamer.Api/"]
COPY ["src/MediaRenamer.Core/MediaRenamer.Core.csproj", "src/MediaRenamer.Core/"]
RUN dotnet restore "src/MediaRenamer.Api/MediaRenamer.Api.csproj"
COPY . .
WORKDIR "/src/src/MediaRenamer.Api"
RUN dotnet build "./MediaRenamer.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MediaRenamer.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MediaRenamer.Api.dll"]
