FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ApplicationEvents/ApplicationEvents.csproj", "ApplicationEvents/"]
COPY ["Contracts/Contracts.csproj", "Contracts/"]
RUN dotnet restore "ApplicationEvents/ApplicationEvents.csproj"
COPY . .
WORKDIR "/src/ApplicationEvents"
RUN dotnet build "./ApplicationEvents.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ApplicationEvents.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ApplicationEvents.dll"]
