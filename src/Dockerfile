# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

# Copy project files
COPY Template/Template.csproj ./Template/
COPY Template.Web/Template.Web.csproj ./Template.Web/

# Restore dependencies
RUN dotnet restore Template.Web/Template.Web.csproj

# Copy source code
COPY Template ./Template/
COPY Template.Web ./Template.Web/

# Build the application
RUN dotnet build -c Release -o /app/build Template.Web/Template.Web.csproj

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

# Install curl for health checks (mysql-client may not be available in all versions)
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy built app from build stage
COPY --from=build /app/build .

# Create required directories (app expects them)
RUN mkdir -p /app/node_modules /app/Areas /app/Features /app/Views /app/bin /app/obj

# Expose ports
EXPOSE 8080 8443

# Set environment
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Docker

# Run the application
ENTRYPOINT ["dotnet", "Template.Web.dll"]
