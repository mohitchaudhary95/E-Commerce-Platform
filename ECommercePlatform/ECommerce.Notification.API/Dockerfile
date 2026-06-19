FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore ECommerce.Notification.API/ECommerce.Notification.API.csproj

RUN dotnet publish ECommerce.Notification.API/ECommerce.Notification.API.csproj \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 5007

ENTRYPOINT ["dotnet", "ECommerce.Notification.API.dll"]