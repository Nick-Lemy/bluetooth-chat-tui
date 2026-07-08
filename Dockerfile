# ---- build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first (cached unless the csproj changes)
COPY server/server.csproj server/
RUN dotnet restore server/server.csproj

# Build and publish
COPY server/ server/
RUN dotnet publish server/server.csproj -c Release -o /app

# ---- runtime stage ----
FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app .

# The server listens on TCP 11000
EXPOSE 11000

ENTRYPOINT ["dotnet", "server.dll"]
