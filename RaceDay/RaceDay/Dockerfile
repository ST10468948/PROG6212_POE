# Stage 1: Runtime base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

# Stage 2: SDK image for compiling
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files using 'RaceDay/' folder path
COPY ["RaceDay/RaceDay.Mvc.csproj", "RaceDay/"]
COPY ["RaceDay.Contracts/RaceDay.Contracts.csproj", "RaceDay.Contracts/"]
RUN dotnet restore "RaceDay/RaceDay.Mvc.csproj"

# Copy the rest of the repository source code and build
COPY . .
WORKDIR "/src/RaceDay"
RUN dotnet build "RaceDay.Mvc.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 3: Publish output
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "RaceDay.Mvc.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 4: Final deployment container
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RaceDay.Mvc.dll"]