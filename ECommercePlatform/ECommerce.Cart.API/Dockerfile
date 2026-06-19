FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore ECommerce.Cart.API/ECommerce.Cart.API.csproj

RUN dotnet publish ECommerce.Cart.API/ECommerce.Cart.API.csproj \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 5003

ENTRYPOINT ["dotnet", "ECommerce.Cart.API.dll"]