FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Microservices/Order/eShop.Order.API/eShop.Order.API.csproj", "Microservices/Order/eShop.Order.API/"]
COPY ["RabbitMQEventBus/RabbitMQEventBus/RabbitMQEventBus.csproj", "RabbitMQEventBus/RabbitMQEventBus/"]
COPY ["Microservices/Order/eShop.Order.Application/eShop.Order.Application.csproj", "Microservices/Order/eShop.Order.Application/"]
COPY ["Microservices/Order/eShop.Order.Domain/eShop.Order.Domain.csproj", "Microservices/Order/eShop.Order.Domain/"]
COPY ["Microservices/Order/eShop.Order.Infrastructure/eShop.Order.Infrastructure.csproj", "Microservices/Order/eShop.Order.Infrastructure/"]
RUN dotnet restore "Microservices/Order/eShop.Order.API/eShop.Order.API.csproj"
COPY . .
WORKDIR "/src/Microservices/Order/eShop.Order.API"
RUN dotnet build "./eShop.Order.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./eShop.Order.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "eShop.Order.API.dll"]
