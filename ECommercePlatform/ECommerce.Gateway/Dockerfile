FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Gateway/ECommerce.Gateway/ECommerce.Gateway.csproj src/Gateway/ECommerce.Gateway/

RUN dotnet restore src/Gateway/ECommerce.Gateway/ECommerce.Gateway.csproj

COPY . .
RUN dotnet publish src/Gateway/ECommerce.Gateway/ECommerce.Gateway.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN adduser --disabled-password --gecos "" appuser
USER appuser
COPY --from=build /app/publish .
EXPOSE 5000
ENTRYPOINT ["dotnet", "ECommerce.Gateway.dll"]
