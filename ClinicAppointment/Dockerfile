# ---- BUILD STAGE ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Healthcare.API/Healthcare.API.csproj Healthcare.API/
COPY Domain/Domain.csproj Domain/
COPY Healthcare.Shared/Healthcare.Shared.csproj Healthcare.Shared/
RUN dotnet restore Healthcare.API/Healthcare.API.csproj

COPY . .
RUN dotnet publish Healthcare.API/Healthcare.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- RUNTIME STAGE ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Healthcare.API.dll"]
