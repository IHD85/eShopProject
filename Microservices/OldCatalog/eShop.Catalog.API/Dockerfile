# eShop.Catalog.API/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["eShop.Catalog.API/eShop.Catalog.API.csproj", "eShop.Catalog.API/"]
COPY ["eShop.Catalog.Infrastructure/eShop.Catalog.Infrastructure.csproj", "eShop.Catalog.Infrastructure/"]
COPY ["eShop.Catalog.Domain/eShop.Catalog.Domain.csproj", "eShop.Catalog.Domain/"]
COPY ["eShop.BuildingBlocks.EventBus/eShop.BuildingBlocks.EventBus.csproj", "eShop.BuildingBlocks.EventBus/"]
RUN dotnet restore "eShop.Catalog.API/eShop.Catalog.API.csproj"

COPY . .
WORKDIR "/src/eShop.Catalog.API"
RUN dotnet build "eShop.Catalog.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "eShop.Catalog.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "eShop.Catalog.API.dll"]
