# Use the official .NET 9 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy solution file
COPY *.sln ./

# Copy project files
COPY src/StonkMarketGame.Bot/*.csproj ./src/StonkMarketGame.Bot/
COPY src/StonkMarketGame.Core/*.csproj ./src/StonkMarketGame.Core/
COPY src/StonkMarketGame.Infrastructure/*.csproj ./src/StonkMarketGame.Infrastructure/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY src/ ./src/

# Build the application
RUN dotnet publish src/StonkMarketGame.Bot/StonkMarketGame.Bot.csproj -c Release -o out

# Use the official .NET 9 runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy the published application
COPY --from=build /app/out .

# Copy configuration files
COPY --from=build /app/src/StonkMarketGame.Bot/appsettings.json .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "StonkMarketGame.Bot.dll"]