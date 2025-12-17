FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
WORKDIR /src/src/Services/Ordering/Ordering.API
RUN dotnet restore
RUN dotnet build --no-restore -c Release -o /app

FROM build AS publish
RUN dotnet publish --no-restore -c Release -o /app

FROM base AS final
WORKDIR /app
RUN apt-get update && apt-get install -y zlib1g && apt-get upgrade -y zlib1g && apt-get clean && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Ordering.API.dll"]
