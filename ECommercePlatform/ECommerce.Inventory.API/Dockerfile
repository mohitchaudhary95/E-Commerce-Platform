FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Shared/ECommerce.Shared.Common/ECommerce.Shared.Common.csproj src/Shared/ECommerce.Shared.Common/
COPY src/Shared/ECommerce.Shared.Contracts/ECommerce.Shared.Contracts.csproj src/Shared/ECommerce.Shared.Contracts/
COPY src/Shared/ECommerce.Shared.RabbitMQ/ECommerce.Shared.RabbitMQ.csproj src/Shared/ECommerce.Shared.RabbitMQ/
COPY src/Services/InventoryService/ECommerce.Inventory.Domain/ECommerce.Inventory.Domain.csproj src/Services/InventoryService/ECommerce.Inventory.Domain/
COPY src/Services/InventoryService/ECommerce.Inventory.Application/ECommerce.Inventory.Application.csproj src/Services/InventoryService/ECommerce.Inventory.Application/
COPY src/Services/InventoryService/ECommerce.Inventory.Infrastructure/ECommerce.Inventory.Infrastructure.csproj src/Services/InventoryService/ECommerce.Inventory.Infrastructure/
COPY src/Services/InventoryService/ECommerce.Inventory.API/ECommerce.Inventory.API.csproj src/Services/InventoryService/ECommerce.Inventory.API/

RUN dotnet restore src/Services/InventoryService/ECommerce.Inventory.API/ECommerce.Inventory.API.csproj

COPY . .
RUN dotnet publish src/Services/InventoryService/ECommerce.Inventory.API/ECommerce.Inventory.API.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN adduser --disabled-password --gecos "" appuser
USER appuser
COPY --from=build /app/publish .
EXPOSE 5006
ENTRYPOINT ["dotnet", "ECommerce.Inventory.API.dll"]
