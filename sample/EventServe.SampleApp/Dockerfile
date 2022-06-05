#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["sample/EventServe.SampleApp/EventServe.SampleApp.csproj", "sample/EventServe.SampleApp/"]
COPY ["src/EventServe.Extensions.Microsoft.DependencyInjection/EventServe.Extensions.Microsoft.DependencyInjection.csproj", "src/EventServe.Extensions.Microsoft.DependencyInjection/"]
COPY ["src/EventServe/EventServe.csproj", "src/EventServe/"]
COPY ["src/EventServe.EventStore/EventServe.EventStore.csproj", "src/EventServe.EventStore/"]
COPY ["src/EventServe.EventStore.Extensions.Microsoft.DependencyInjection/EventServe.EventStore.Extensions.Microsoft.DepdendencyInjection.csproj", "src/EventServe.EventStore.Extensions.Microsoft.DependencyInjection/"]
RUN dotnet restore "sample/EventServe.SampleApp/EventServe.SampleApp.csproj"
COPY . .
WORKDIR "/src/sample/EventServe.SampleApp"
RUN dotnet build "EventServe.SampleApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EventServe.SampleApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventServe.SampleApp.dll"]