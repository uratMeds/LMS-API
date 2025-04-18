# --- Base image ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
ARG APP_UID=1000
USER $APP_UID
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# --- Build image ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["LMS API/LMS API.csproj","LMS API/"]
RUN dotnet restore "LMS API/LMS API.csproj"
COPY . .
WORKDIR "/src/LMS API"
RUN dotnet build "LMS API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# --- Publish image ---
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "LMS API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# --- Final image ---
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ["LMS API/LMS.db","./LMS.db"]
ENTRYPOINT ["dotnet","LMS API.dll"]
