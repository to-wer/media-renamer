FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/MediaRenamer.Web/MediaRenamer.Web.csproj", "src/MediaRenamer.Web/"]
COPY ["src/MediaRenamer.Core/MediaRenamer.Core.csproj", "src/MediaRenamer.Core/"]
RUN dotnet restore "src/MediaRenamer.Web/MediaRenamer.Web.csproj"
COPY . .
WORKDIR "/src/src/MediaRenamer.Web"
RUN dotnet build "./MediaRenamer.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MediaRenamer.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MediaRenamer.Web.dll"]
