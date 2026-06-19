FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore ECommerce.Order.API/ECommerce.Order.API.csproj

RUN dotnet publish ECommerce.Order.API/ECommerce.Order.API.csproj \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 5004

ENTRYPOINT ["dotnet", "ECommerce.Order.API.dll"]