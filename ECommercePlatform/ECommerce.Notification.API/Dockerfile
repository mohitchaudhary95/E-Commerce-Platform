FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Shared/ECommerce.Shared.Common/ECommerce.Shared.Common.csproj src/Shared/ECommerce.Shared.Common/
COPY src/Shared/ECommerce.Shared.Contracts/ECommerce.Shared.Contracts.csproj src/Shared/ECommerce.Shared.Contracts/
COPY src/Shared/ECommerce.Shared.RabbitMQ/ECommerce.Shared.RabbitMQ.csproj src/Shared/ECommerce.Shared.RabbitMQ/
COPY src/Services/NotificationService/ECommerce.Notification.Application/ECommerce.Notification.Application.csproj src/Services/NotificationService/ECommerce.Notification.Application/
COPY src/Services/NotificationService/ECommerce.Notification.Infrastructure/ECommerce.Notification.Infrastructure.csproj src/Services/NotificationService/ECommerce.Notification.Infrastructure/
COPY src/Services/NotificationService/ECommerce.Notification.API/ECommerce.Notification.API.csproj src/Services/NotificationService/ECommerce.Notification.API/

RUN dotnet restore src/Services/NotificationService/ECommerce.Notification.API/ECommerce.Notification.API.csproj

COPY . .
RUN dotnet publish src/Services/NotificationService/ECommerce.Notification.API/ECommerce.Notification.API.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN adduser --disabled-password --gecos "" appuser
USER appuser
COPY --from=build /app/publish .
EXPOSE 5007
ENTRYPOINT ["dotnet", "ECommerce.Notification.API.dll"]
