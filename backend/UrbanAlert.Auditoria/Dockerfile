FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY UrbanAlert.Auditoria.csproj ./
RUN dotnet restore UrbanAlert.Auditoria.csproj
COPY . ./
RUN dotnet publish UrbanAlert.Auditoria.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "UrbanAlert.Auditoria.dll"]
