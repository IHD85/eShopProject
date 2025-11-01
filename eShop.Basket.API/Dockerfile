# ===== Stage 1: Base image =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# ===== Stage 2: Build =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .
WORKDIR "/src/eShop.Basket.API"

RUN dotnet restore
RUN dotnet build -c Release -o /app/build

# ===== Stage 3: Publish =====
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# ===== Stage 4: Final =====
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "eShop.Basket.API.dll"]
