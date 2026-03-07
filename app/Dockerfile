FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Rpa.Domain/Rpa.Domain.csproj src/Rpa.Domain/
COPY src/Rpa.Application/Rpa.Application.csproj src/Rpa.Application/
COPY src/Rpa.Infrastructure/Rpa.Infrastructure.csproj src/Rpa.Infrastructure/
COPY src/RPA.Worker/RPA.Worker.csproj src/RPA.Worker/
RUN dotnet restore src/RPA.Worker/RPA.Worker.csproj

COPY . .
RUN dotnet publish src/RPA.Worker/RPA.Worker.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

VOLUME ["/data"]
ENV Sqlite__ConnectionString="Data Source=/data/rpa_scraping.db;Cache=Shared"

ENTRYPOINT ["dotnet", "RPA.Worker.dll"]
