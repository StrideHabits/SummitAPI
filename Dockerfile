# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj and restore as distinct layers
COPY Summit/SummitAPI.csproj Summit/
RUN dotnet restore Summit/SummitAPI.csproj

# copy everything else and build
COPY . .
WORKDIR /src/Summit
RUN dotnet publish SummitAPI.csproj -c Release -o /app/out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Render injects $PORT
ENV ASPNETCORE_URLS=http://+:$PORT
EXPOSE 10000

ENTRYPOINT ["dotnet", "SummitAPI.dll"]
