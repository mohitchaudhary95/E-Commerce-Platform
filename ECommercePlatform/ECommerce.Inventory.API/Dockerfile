FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore ECommerce.Inventory.API/ECommerce.Inventory.API.csproj

RUN dotnet publish ECommerce.Inventory.API/ECommerce.Inventory.API.csproj \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 5006

ENTRYPOINT ["dotnet", "ECommerce.Inventory.API.dll"]