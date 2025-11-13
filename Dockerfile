# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY BlazorStateApp/BlazorStateApp.csproj BlazorStateApp/
RUN dotnet restore BlazorStateApp/BlazorStateApp.csproj

# Copy the rest of the source code
COPY BlazorStateApp/ BlazorStateApp/

# Build the application
WORKDIR /src/BlazorStateApp
RUN dotnet build BlazorStateApp.csproj -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish BlazorStateApp.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published application
COPY --from=publish /app/publish .

# Set environment variable for production
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "BlazorStateApp.dll"]
