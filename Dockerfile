FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Node.js kurulumu
RUN apt-get update && apt-get install -y curl
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash -
RUN apt-get install -y nodejs

# Projeyi kopyala
COPY . .

# Angular uygulamasını derle
WORKDIR /app/jobanalyzerdashboard.client
RUN npm install
RUN npm run build -- --configuration production

# Backend'i derle
WORKDIR /app
RUN dotnet publish -c Release -o out JobAnalyzerDashboard.Server/JobAnalyzerDashboard.Server.csproj

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Port ayarı
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Uygulamayı çalıştır
ENTRYPOINT ["dotnet", "JobAnalyzerDashboard.Server.dll"]
