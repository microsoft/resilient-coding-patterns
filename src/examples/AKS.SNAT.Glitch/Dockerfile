# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY SNAT.Glitch.csproj ./
RUN dotnet restore

# Copy the remaining source code
COPY . ./

# Build the application
# Build and publish the main project
RUN dotnet publish -c Release -o out SNAT.Glitch.csproj

# Use the official .NET runtime image for the app
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Set the entrypoint
ENTRYPOINT ["dotnet", "SNAT.Glitch.dll"]
