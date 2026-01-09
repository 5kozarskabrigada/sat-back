# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["SatMockPlatform.Api/SatMockPlatform.Api.csproj", "SatMockPlatform.Api/"]
RUN dotnet restore "SatMockPlatform.Api/SatMockPlatform.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/SatMockPlatform.Api"
RUN dotnet build "SatMockPlatform.Api.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "SatMockPlatform.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "SatMockPlatform.Api.dll"]
