FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["ArcadeProject.csproj", "./"]
RUN dotnet restore "ArcadeProject.csproj"

COPY . .
RUN dotnet build "ArcadeProject.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ArcadeProject.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

FROM base AS final
WORKDIR /app


COPY --from=publish /app/publish .

RUN mkdir -p /Data && chmod 777 /Data

ENTRYPOINT ["dotnet", "ArcadeProject.dll"]
