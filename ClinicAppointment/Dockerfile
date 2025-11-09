# ---- BUILD STAGE ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj-երը՝ արագ restore-ի համար
COPY ClinicAppointment/ClinicAppointment.csproj ClinicAppointment/
COPY Application/Application.csproj Application/
COPY Domain/Domain.csproj Domain/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
RUN dotnet restore ClinicAppointment/ClinicAppointment.csproj

# copy ամբողջ լուծումը և publish
COPY . .
RUN dotnet publish ClinicAppointment/ClinicAppointment.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- RUNTIME STAGE ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# ENV-երը կարդացվում են Program.cs-ից
ENV ASPNETCORE_URLS=http://+:8080
ENV RABBITMQ_HOST=rabbitmq
ENV ConnectionStrings__Default="Server=sqlserver;Database=HealthcareDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"

COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ClinicAppointment.dll"]
