# Contexto de build: /Web (no esta carpeta) — así puede copiar los proyectos
# compartidos. Ej.: docker build -f Apps/SaborByte.Web.Cocina/Dockerfile -t sabor-byte-cocina .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Compartido/ Compartido/
COPY Apps/SaborByte.Web.Cocina/ Apps/SaborByte.Web.Cocina/

RUN dotnet publish Apps/SaborByte.Web.Cocina/SaborByte.Web.Cocina.csproj -c Release -o /app/publish

FROM nginx:alpine AS final
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY nginx-spa.conf /etc/nginx/conf.d/default.conf

EXPOSE 8080
