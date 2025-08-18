# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

# Render sets PORT; bind Kestrel to it
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

# Ensure persistent folders exist (Render will mount /data disk from blueprint)
RUN mkdir -p /data/uploads

# (Optional) expose a dummy port for local docker run
EXPOSE 8080

ENTRYPOINT ["dotnet", "HabitTrackerAPI.dll"]
