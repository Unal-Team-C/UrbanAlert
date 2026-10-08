# Contexto de build: backend/ (Auditoria referencia UrbanAlert.Reportes/src/Application)
#   docker build -f UrbanAlert.Auditoria/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY UrbanAlert.Reportes/src/Domain/Domain.csproj UrbanAlert.Reportes/src/Domain/
COPY UrbanAlert.Reportes/src/Application/Application.csproj UrbanAlert.Reportes/src/Application/
COPY UrbanAlert.Auditoria/UrbanAlert.Auditoria.csproj UrbanAlert.Auditoria/
RUN dotnet restore UrbanAlert.Auditoria/UrbanAlert.Auditoria.csproj

COPY UrbanAlert.Reportes/src/Domain/ UrbanAlert.Reportes/src/Domain/
COPY UrbanAlert.Reportes/src/Application/ UrbanAlert.Reportes/src/Application/
COPY UrbanAlert.Auditoria/ UrbanAlert.Auditoria/
RUN dotnet publish UrbanAlert.Auditoria/UrbanAlert.Auditoria.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "UrbanAlert.Auditoria.dll"]
