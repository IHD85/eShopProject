FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Microservices/Basket/eShop.Basket.API/eShop.Basket.API.csproj", "Microservices/Basket/eShop.Basket.API/"]
COPY ["RabbitMQEventBus/RabbitMQEventBus/RabbitMQEventBus.csproj", "RabbitMQEventBus/RabbitMQEventBus/"]
COPY ["Microservices/Basket/eShop.Basket.Application/eShop.Basket.Application.csproj", "Microservices/Basket/eShop.Basket.Application/"]
COPY ["Microservices/Basket/eShop.Basket.Infrastructure/eShop.Basket.Infrastructure.csproj", "Microservices/Basket/eShop.Basket.Infrastructure/"]
COPY ["Microservices/Basket/eShop.Basket.Domain/eShop.Basket.Domain.csproj", "Microservices/Basket/eShop.Basket.Domain/"]
RUN dotnet restore "Microservices/Basket/eShop.Basket.API/eShop.Basket.API.csproj"
COPY . .
WORKDIR "/src/Microservices/Basket/eShop.Basket.API"
RUN dotnet build "./eShop.Basket.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./eShop.Basket.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "eShop.Basket.API.dll"]
