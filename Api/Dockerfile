FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Dominio/ Dominio/
COPY Aplicacion/ Aplicacion/
COPY Infraestructura/ Infraestructura/
COPY FacturacionElectronica/ FacturacionElectronica/
COPY Api/ Api/

RUN dotnet publish Api/SaborByte.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# La cadena de conexión, la clave JWT y la configuración de FacturacionElectronica
# se inyectan en producción vía variables de entorno o un vault de secretos — NUNCA
# copiando appsettings.Development.json a la imagen (por eso no se incluye aquí).
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SaborByte.Api.dll"]
