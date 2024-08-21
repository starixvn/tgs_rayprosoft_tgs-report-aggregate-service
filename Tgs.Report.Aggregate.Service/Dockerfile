FROM hub.lumigame.com/base/dotnet/core/sdk:8.0 AS builder

ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.Report.Aggregate.Service/Tgs.Report.Aggregate.Service.csproj", "Tgs.Report.Aggregate.Service/"]
COPY ["Tgs.Report.Aggregate.Core/Tgs.Report.Aggregate.Core.csproj", "Tgs.Report.Aggregate.Core/"]
COPY ["Tgs.Report.Aggregate.Domain/Tgs.Report.Aggregate.Domain.csproj", "Tgs.Report.Aggregate.Domain/"]
COPY ["Tgs.Report.Aggregate.Repository/Tgs.Report.Aggregate.Repository.csproj", "Tgs.Report.Aggregate.Repository/"]
RUN dotnet restore "./Tgs.Report.Aggregate.Service/./Tgs.Report.Aggregate.Service.csproj"
COPY . .
WORKDIR "/src/Tgs.Report.Aggregate.Service"
RUN dotnet publish "./Tgs.Report.Aggregate.Service.csproj" -c $BUILD_CONFIGURATION -o /app /p:UseAppHost=false

FROM hub.lumigame.com/base/dotnet/core/aspnet:8.0-alpine-24h1.1 AS base
WORKDIR /app
COPY --from=builder /app .

ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "Tgs.Report.Aggregate.Service.dll"]