# Stage 1: Build and publish the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers to leverage Docker cache
COPY ["api.csproj", "."]
RUN dotnet restore "./api.csproj"

# Copy everything else and build
COPY . .
RUN dotnet publish "api.csproj" -c Release -o /app/publish

# Stage 2: Create the final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/publish .

# Set the listening address to all IPs to be accessible within Kubernetes
ENV ASPNETCORE_URLS=http://+:80

EXPOSE 80
ENTRYPOINT ["dotnet", "api.dll"]
