# Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["Scanner.csproj", "."]
RUN dotnet restore "Scanner.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "Scanner.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "Scanner.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Scanner.dll"]
