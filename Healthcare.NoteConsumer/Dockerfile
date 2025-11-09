# ---- BUILD STAGE ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy միայն այն project-ները, որոնցից կախված է consumer-ը
COPY Healthcare.NoteConsumer/Healthcare.NoteConsumer.csproj Healthcare.NoteConsumer/
COPY Domain/Domain.csproj Domain/
RUN dotnet restore Healthcare.NoteConsumer/Healthcare.NoteConsumer.csproj

# Copy ամբողջ source code-ը և publish
COPY . .
RUN dotnet publish Healthcare.NoteConsumer/Healthcare.NoteConsumer.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- RUNTIME STAGE ----
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Healthcare.NoteConsumer.dll"]