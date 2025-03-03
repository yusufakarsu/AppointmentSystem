#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

# Use Linux-based .NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Install PostgreSQL (Optional: Only needed if running inside the same container)
FROM postgres:latest AS postgres
ENV POSTGRES_USER=postgres
ENV POSTGRES_PASSWORD=mypassword123!
ENV POSTGRES_DB=coding-challenge
EXPOSE 5432

# Build step using Linux-based .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AppointmentSystem.Api/AppointmentSystem.Api.csproj", "AppointmentSystem.Api/"]
RUN dotnet restore "./AppointmentSystem.Api/AppointmentSystem.Api.csproj"
COPY . .
WORKDIR "/src/AppointmentSystem.Api"
RUN dotnet build "./AppointmentSystem.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish step
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AppointmentSystem.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime container
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AppointmentSystem.Api.dll"]