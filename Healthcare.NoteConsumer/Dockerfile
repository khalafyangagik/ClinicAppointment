# ---- BUILD ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Healthcare.NoteConsumer/Healthcare.NoteConsumer.csproj Healthcare.NoteConsumer/
COPY Domain/Domain.csproj Domain/
COPY Healthcare.Shared/Healthcare.Shared.csproj Healthcare.Shared/
RUN dotnet restore Healthcare.NoteConsumer/Healthcare.NoteConsumer.csproj

COPY . .
RUN dotnet publish Healthcare.NoteConsumer/Healthcare.NoteConsumer.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- RUNTIME ----
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Healthcare.NoteConsumer.dll"]
