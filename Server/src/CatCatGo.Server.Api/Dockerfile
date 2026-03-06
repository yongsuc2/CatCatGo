FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/CatCatGo.Shared/CatCatGo.Shared.csproj src/CatCatGo.Shared/
COPY src/CatCatGo.Server.Core/CatCatGo.Server.Core.csproj src/CatCatGo.Server.Core/
COPY src/CatCatGo.Server.Infrastructure/CatCatGo.Server.Infrastructure.csproj src/CatCatGo.Server.Infrastructure/
COPY src/CatCatGo.Server.Api/CatCatGo.Server.Api.csproj src/CatCatGo.Server.Api/
RUN dotnet restore src/CatCatGo.Server.Api/CatCatGo.Server.Api.csproj

COPY src/ src/
RUN dotnet publish src/CatCatGo.Server.Api/CatCatGo.Server.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "CatCatGo.Server.Api.dll"]
