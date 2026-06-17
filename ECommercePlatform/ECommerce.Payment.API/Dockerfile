FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Shared/ECommerce.Shared.Common/ECommerce.Shared.Common.csproj src/Shared/ECommerce.Shared.Common/
COPY src/Shared/ECommerce.Shared.Contracts/ECommerce.Shared.Contracts.csproj src/Shared/ECommerce.Shared.Contracts/
COPY src/Shared/ECommerce.Shared.RabbitMQ/ECommerce.Shared.RabbitMQ.csproj src/Shared/ECommerce.Shared.RabbitMQ/
COPY src/Services/PaymentService/ECommerce.Payment.Domain/ECommerce.Payment.Domain.csproj src/Services/PaymentService/ECommerce.Payment.Domain/
COPY src/Services/PaymentService/ECommerce.Payment.Application/ECommerce.Payment.Application.csproj src/Services/PaymentService/ECommerce.Payment.Application/
COPY src/Services/PaymentService/ECommerce.Payment.Infrastructure/ECommerce.Payment.Infrastructure.csproj src/Services/PaymentService/ECommerce.Payment.Infrastructure/
COPY src/Services/PaymentService/ECommerce.Payment.API/ECommerce.Payment.API.csproj src/Services/PaymentService/ECommerce.Payment.API/

RUN dotnet restore src/Services/PaymentService/ECommerce.Payment.API/ECommerce.Payment.API.csproj

COPY . .
RUN dotnet publish src/Services/PaymentService/ECommerce.Payment.API/ECommerce.Payment.API.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN adduser --disabled-password --gecos "" appuser
USER appuser
COPY --from=build /app/publish .
EXPOSE 5005
ENTRYPOINT ["dotnet", "ECommerce.Payment.API.dll"]
