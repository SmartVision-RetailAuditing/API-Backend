# =========================
# Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Projeyi kopyala ve restore et
COPY *.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app

# =========================
# Runtime stage
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Build çıktısını runtime’a kopyala
COPY --from=build /app .

# ASP.NET Core URL
ENV ASPNETCORE_URLS=http://+:8080

# Port expose
EXPOSE 8080

# Uygulamayı çalıştır
ENTRYPOINT ["dotnet", "ApiBackend.dll"]
