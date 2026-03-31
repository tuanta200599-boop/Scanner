# Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy csproj và restore trước (tối ưu cache)
COPY Scanner.csproj ./
RUN dotnet restore

# Copy toàn bộ source
COPY . ./

# Build
RUN dotnet build -c Release -o /app/build

# Publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app

# Copy từ build sang runtime
COPY --from=build /app/publish ./

# Run app
ENTRYPOINT ["dotnet", "Scanner.dll"]