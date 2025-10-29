# ===== Stage 1: base image =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# ===== Stage 2: build =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
WORKDIR "/src/Gateway.Api"
RUN dotnet restore "Gateway.Api.csproj"
RUN dotnet build "Gateway.Api.csproj" -c Release -o /app/build

# ===== Stage 3: publish =====
FROM build AS publish
RUN dotnet publish "Gateway.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ===== Stage 4: final runtime image =====
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gateway.Api.dll"]
