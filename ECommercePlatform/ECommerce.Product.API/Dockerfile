FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore ECommerce.Product.API/ECommerce.Product.API.csproj

RUN dotnet publish ECommerce.Product.API/ECommerce.Product.API.csproj \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 5002

ENTRYPOINT ["dotnet", "ECommerce.Product.API.dll"]