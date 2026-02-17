FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props DeliveryApp.sln ./
COPY src/DeliveryApp.Domain/DeliveryApp.Domain.csproj src/DeliveryApp.Domain/
COPY src/DeliveryApp.Application/DeliveryApp.Application.csproj src/DeliveryApp.Application/
COPY src/DeliveryApp.Data/DeliveryApp.Data.csproj src/DeliveryApp.Data/
COPY src/DeliveryApp.Adapter/DeliveryApp.Adapter.csproj src/DeliveryApp.Adapter/
RUN dotnet restore src/DeliveryApp.Adapter/DeliveryApp.Adapter.csproj

COPY src/ src/
RUN dotnet publish src/DeliveryApp.Adapter/DeliveryApp.Adapter.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "DeliveryApp.Adapter.dll"]
