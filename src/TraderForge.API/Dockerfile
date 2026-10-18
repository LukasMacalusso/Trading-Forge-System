# 1. Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

# 2. Build image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy all the .csproj files and restore dependencies
COPY ["src/TraderForge.API/TraderForge.API.csproj", "src/TraderForge.API/"]
COPY ["src/TraderForge.Application/TraderForge.Application.csproj", "src/TraderForge.Application/"]
COPY ["src/TraderForge.Domain/TraderForge.Domain.csproj", "src/TraderForge.Domain/"]
COPY ["src/TraderForge.Infrastructure/TraderForge.Infrastructure.csproj", "src/TraderForge.Infrastructure/"]
RUN dotnet restore "src/TraderForge.API/TraderForge.API.csproj"

# Copy the rest of the code
COPY . .

# Build the API
WORKDIR "/src/src/TraderForge.API"
RUN dotnet build "TraderForge.API.csproj" -c Release -o /app/build

# 3. Publish the application
FROM build AS publish
RUN dotnet publish "TraderForge.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 4. Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TraderForge.API.dll"]