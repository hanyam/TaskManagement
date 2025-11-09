FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/TaskManagement.Api/TaskManagement.Api.csproj", "src/TaskManagement.Api/"]
COPY ["src/TaskManagement.Application/TaskManagement.Application.csproj", "src/TaskManagement.Application/"]
COPY ["src/TaskManagement.Domain/TaskManagement.Domain.csproj", "src/TaskManagement.Domain/"]
COPY ["src/TaskManagement.Infrastructure/TaskManagement.Infrastructure.csproj", "src/TaskManagement.Infrastructure/"]
RUN dotnet restore "src/TaskManagement.Api/TaskManagement.Api.csproj"
COPY . .
WORKDIR "/src/src/TaskManagement.Api"
RUN dotnet build "./TaskManagement.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TaskManagement.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskManagement.Api.dll"]
