FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Gateway/API.Gateway/API.Gateway.csproj", "Gateway/API.Gateway/"]
RUN dotnet restore "Gateway/API.Gateway/API.Gateway.csproj"
COPY . .
WORKDIR "/src/Gateway/API.Gateway"
RUN dotnet build "./API.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./API.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "API.Gateway.dll"]
