# syntax=docker/dockerfile:1.7

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /workspace
# The orders solution can use local BuildingBlocks rather than a private package feed.
COPY --from=buildingblocks / /workspace/fcg-dotnet-building-blocks
COPY . /workspace/fcg-orders-service
WORKDIR /workspace/fcg-orders-service

RUN --mount=type=secret,id=nuget_config,target=/root/.nuget/NuGet/NuGet.Config,required=true \
    dotnet restore src/FiapCloudGames.Orders.Api/FiapCloudGames.Orders.Api.csproj -p:UseLocalBuildingBlocks=true --configfile /root/.nuget/NuGet/NuGet.Config && \
    dotnet restore src/FiapCloudGames.Orders.Worker/FiapCloudGames.Orders.Worker.csproj -p:UseLocalBuildingBlocks=true --configfile /root/.nuget/NuGet/NuGet.Config && \
    dotnet restore src/FiapCloudGames.Orders.Migrations/FiapCloudGames.Orders.Migrations.csproj -p:UseLocalBuildingBlocks=true --configfile /root/.nuget/NuGet/NuGet.Config

RUN dotnet publish src/FiapCloudGames.Orders.Api/FiapCloudGames.Orders.Api.csproj -c Release -p:UseLocalBuildingBlocks=true --no-restore -o /out/api && \
    dotnet publish src/FiapCloudGames.Orders.Worker/FiapCloudGames.Orders.Worker.csproj -c Release -p:UseLocalBuildingBlocks=true --no-restore -o /out/worker && \
    dotnet publish src/FiapCloudGames.Orders.Migrations/FiapCloudGames.Orders.Migrations.csproj -c Release -p:UseLocalBuildingBlocks=true --no-restore -o /out/migrations

FROM runtime AS migrations
COPY --from=build /out/migrations .
ENTRYPOINT ["dotnet", "FiapCloudGames.Orders.Migrations.dll"]

FROM runtime AS worker
COPY --from=build /out/worker .
ENTRYPOINT ["dotnet", "FiapCloudGames.Orders.Worker.dll"]

FROM runtime AS final
COPY --from=build /out/api .
ENTRYPOINT ["dotnet", "FiapCloudGames.Orders.Api.dll"]
