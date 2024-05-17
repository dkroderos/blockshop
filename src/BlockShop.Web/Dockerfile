FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 25627
EXPOSE 25628

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/BlockShop.Web/BlockShop.Web.csproj", "./BlockShop.Web/"]
RUN dotnet restore "./BlockShop.Web/BlockShop.Web.csproj"
COPY . .
WORKDIR "/src/BlockShop.Web"
RUN dotnet build "./BlockShop.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BlockShop.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BlockShop.Web.dll"]