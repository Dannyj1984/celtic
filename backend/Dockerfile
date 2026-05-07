# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["src/Celtic.Api/Celtic.Api.csproj", "Celtic.Api/"]
RUN dotnet restore "Celtic.Api/Celtic.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/Celtic.Api"
RUN dotnet build "Celtic.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Celtic.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose port 80 (or 8080 depending on preference)
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "Celtic.Api.dll"]
