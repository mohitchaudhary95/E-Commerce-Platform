FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore ECommerce.Payment.API/ECommerce.Payment.API.csproj

RUN dotnet publish ECommerce.Payment.API/ECommerce.Payment.API.csproj \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 5005

ENTRYPOINT ["dotnet", "ECommerce.Payment.API.dll"]