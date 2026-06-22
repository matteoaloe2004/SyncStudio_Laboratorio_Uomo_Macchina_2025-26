# Stage 1: Build static assets using Node
FROM node:20 AS node-build

WORKDIR /app

# Copy package files and install dependencies
COPY Template.Web/package*.json ./Template.Web/
RUN cd Template.Web && npm install

# Copy source code and compile SASS/bundling
COPY Template.Web/wwwroot ./Template.Web/wwwroot/
COPY Template.Web/compile-assets.js ./Template.Web/
RUN cd Template.Web && node compile-assets.js

# Stage 2: Build dotnet application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

# Copy project files and restore dotnet packages
COPY Template/Template.csproj ./Template/
COPY Template.Web/Template.Web.csproj ./Template.Web/
RUN dotnet restore Template.Web/Template.Web.csproj

# Copy dotnet source code
COPY Template ./Template/
COPY Template.Web ./Template.Web/

# Copy compiled CSS assets from Node build stage
COPY --from=node-build /app/Template.Web/wwwroot/css ./Template.Web/wwwroot/css

# Build & Publish the application
RUN dotnet publish -c Release -o /app/publish Template.Web/Template.Web.csproj

# Stage 3: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published files from dotnet build stage
COPY --from=build /app/publish .

# Copy node_modules from node-build stage to serve FontAwesome and other assets
COPY --from=node-build /app/Template.Web/node_modules ./node_modules

# Create additional empty folders expected by the app
RUN mkdir -p /app/Areas /app/Features /app/Views /app/bin /app/obj

EXPOSE 8080 8443

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Docker

ENTRYPOINT ["dotnet", "Template.Web.dll"]
