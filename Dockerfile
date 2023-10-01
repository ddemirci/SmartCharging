# Build stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy and restore main project
COPY ["SmartCharging.API/SmartCharging.API.csproj", "SmartCharging.API/"]
RUN dotnet restore "SmartCharging.API/SmartCharging.API.csproj"

# Copy and restore supporting projects
COPY ["SmartCharging.Domain/SmartCharging.Domain.csproj", "SmartCharging.Domain/"]
COPY ["SmartCharging.Persistence/SmartCharging.Persistence.csproj", "SmartCharging.Persistence/"]
COPY ["SmartCharging.Repository/SmartCharging.Repository.csproj", "SmartCharging.Repository/"]
COPY ["SmartCharging.Service/SmartCharging.Service.csproj", "SmartCharging.Service/"]
RUN dotnet restore "SmartCharging.API/SmartCharging.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/SmartCharging.API"
RUN dotnet build "SmartCharging.API.csproj" -c Release -o /app/build

# Publish the API
FROM build AS publish
RUN dotnet publish "SmartCharging.API.csproj" -c Release -o /app/publish

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app
EXPOSE 80

# Copy the published files from build stage
COPY --from=publish /app/publish .

# Set the entry point for your API
ENTRYPOINT ["dotnet", "SmartCharging.API.dll"]
