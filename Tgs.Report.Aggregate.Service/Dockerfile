#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
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
RUN dotnet build "./Tgs.Report.Aggregate.Service.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Tgs.Report.Aggregate.Service.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.Report.Aggregate.Service.dll"]