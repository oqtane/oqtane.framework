# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
# user and group id for the app user, can be set via the .env file or as parameter in docker build
# default is 1000 which is the default user id for the first user on a Linux system
USER $APP_UID
WORKDIR /app
# The ASP.NET Core Module creates a handler that listens on port 8080 when present. 
# If it is not present, the default is to listen on port 80.
# this value need to match the port in docker-compose.yml and ENV ASPNETCORE_URLS resp. ASPNETCORE_HTTPS_PORTS in the .env file
EXPOSE 8080
RUN echo "user: $APP_UID"

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
RUN echo "BUILD_CONFIGURATION: $BUILD_CONFIGURATION"
# Runtime Identifier, for details see https://learn.microsoft.com/en-us/dotnet/core/rid-catalog#known-rids
ARG RUNTIME_IDENTIFIER=linux-x64
RUN echo "RUNTIME_IDENTIFIER: $RUNTIME_IDENTIFIER"
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["Oqtane.Server/Oqtane.Server.csproj", "Oqtane.Server/"]
COPY ["Oqtane.Client/Oqtane.Client.csproj", "Oqtane.Client/"]
COPY ["Oqtane.Shared/Oqtane.Shared.csproj", "Oqtane.Shared/"]
RUN dotnet restore "./Oqtane.Server/Oqtane.Server.csproj"
COPY . .
WORKDIR "/src/Oqtane.Server"
# We use no-self-contained to avoid copying the .NET runtime to the image, which is already present in the base image. 
# This results in a smaller image size.
RUN dotnet build "./Oqtane.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build -r $RUNTIME_IDENTIFIER --no-self-contained  

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG RUNTIME_IDENTIFIER=linux-x64
RUN dotnet publish "./Oqtane.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false -r $RUNTIME_IDENTIFIER --no-self-contained

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Oqtane.Server.dll"]