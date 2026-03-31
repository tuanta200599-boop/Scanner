FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Sao chép file project trước để tận dụng cache
COPY ["Scanner/Scanner.csproj", "Scanner/"]
RUN dotnet restore "Scanner/Scanner.csproj"

# Sao chép toàn bộ mã nguồn từ thư mục gốc
COPY . .

# Chuyển vào thư mục chứa project và build
WORKDIR "/src/Scanner"
RUN dotnet build "Scanner.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Scanner.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Tạo thư mục lưu chìa khóa DataProtection và cấp quyền
RUN mkdir /keys && chmod 777 /keys

ENTRYPOINT ["dotnet", "Scanner.dll"]