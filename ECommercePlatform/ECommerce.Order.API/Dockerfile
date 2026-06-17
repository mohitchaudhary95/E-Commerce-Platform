FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Shared/ECommerce.Shared.Common/ECommerce.Shared.Common.csproj src/Shared/ECommerce.Shared.Common/
COPY src/Shared/ECommerce.Shared.Contracts/ECommerce.Shared.Contracts.csproj src/Shared/ECommerce.Shared.Contracts/
COPY src/Shared/ECommerce.Shared.RabbitMQ/ECommerce.Shared.RabbitMQ.csproj src/Shared/ECommerce.Shared.RabbitMQ/
COPY src/Services/OrderService/ECommerce.Order.Domain/ECommerce.Order.Domain.csproj src/Services/OrderService/ECommerce.Order.Domain/
COPY src/Services/OrderService/ECommerce.Order.Application/ECommerce.Order.Application.csproj src/Services/OrderService/ECommerce.Order.Application/
COPY src/Services/OrderService/ECommerce.Order.Infrastructure/ECommerce.Order.Infrastructure.csproj src/Services/OrderService/ECommerce.Order.Infrastructure/
COPY src/Services/OrderService/ECommerce.Order.API/ECommerce.Order.API.csproj src/Services/OrderService/ECommerce.Order.API/

RUN dotnet restore src/Services/OrderService/ECommerce.Order.API/ECommerce.Order.API.csproj

COPY . .
RUN dotnet publish src/Services/OrderService/ECommerce.Order.API/ECommerce.Order.API.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN adduser --disabled-password --gecos "" appuser
USER appuser
COPY --from=build /app/publish .
EXPOSE 5004
ENTRYPOINT ["dotnet", "ECommerce.Order.API.dll"]
