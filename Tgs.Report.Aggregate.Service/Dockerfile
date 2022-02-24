#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BuildConfiguration=Release
RUN --mount=type=secret,id=nuget,dst=/pat cp /pat ~/.nuget/NuGet/NuGet.Config
RUN dotnet nuget remove source 'Microsoft Visual Studio Offline Packages' || true
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.Report.Aggregate.Service/Tgs.Report.Aggregate.Service.csproj", "Tgs.Report.Aggregate.Service/"]
COPY ["Tgs.Report.Aggregate.Core/Tgs.Report.Aggregate.Core.csproj", "Tgs.Report.Aggregate.Core/"]
COPY ["Tgs.Report.Aggregate.Domain/Tgs.Report.Aggregate.Domain.csproj", "Tgs.Report.Aggregate.Domain/"]
COPY ["Tgs.Report.Aggregate.Repository/Tgs.Report.Aggregate.Repository.csproj", "Tgs.Report.Aggregate.Repository/"]
RUN dotnet restore "Tgs.Report.Aggregate.Service/Tgs.Report.Aggregate.Service.csproj"
COPY . .
WORKDIR "/src/Tgs.Report.Aggregate.Service"
RUN dotnet build "Tgs.Report.Aggregate.Service.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "Tgs.Report.Aggregate.Service.csproj" -c $BuildConfiguration -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.Report.Aggregate.Service.dll"]