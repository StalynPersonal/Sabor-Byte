# Contexto de build: /Web (no esta carpeta) — así puede copiar los proyectos
# compartidos. Ej.: docker build -f Apps/SaborByte.Web.Caja/Dockerfile -t sabor-byte-caja .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Compartido/ Compartido/
COPY Apps/SaborByte.Web.Caja/ Apps/SaborByte.Web.Caja/

RUN dotnet publish Apps/SaborByte.Web.Caja/SaborByte.Web.Caja.csproj -c Release -o /app/publish

FROM nginx:alpine AS final
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY nginx-spa.conf /etc/nginx/conf.d/default.conf

# El ApiBaseUrl real de producción se define en wwwroot/appsettings.Production.json
# (o reemplazando el archivo al desplegar) — no se hornea en la imagen de desarrollo.
EXPOSE 8080
