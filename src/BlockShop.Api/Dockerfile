FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 25625
EXPOSE 25626

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
RUN mkdir /db
COPY ["src/BlockShop.Api/BlockShop.Api.csproj", "./BlockShop.Api/"]
RUN dotnet restore "./BlockShop.Api/BlockShop.Api.csproj"
COPY "src/" .
WORKDIR "/src/BlockShop.Api"
RUN dotnet build "./BlockShop.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BlockShop.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BlockShop.Api.dll"]