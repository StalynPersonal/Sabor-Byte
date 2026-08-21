# Contexto de build: /Web (no esta carpeta) — así puede copiar los proyectos
# compartidos. Ej.: docker build -f Apps/SaborByte.Web.Mesero/Dockerfile -t sabor-byte-mesero .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Compartido/ Compartido/
COPY Apps/SaborByte.Web.Mesero/ Apps/SaborByte.Web.Mesero/

RUN dotnet publish Apps/SaborByte.Web.Mesero/SaborByte.Web.Mesero.csproj -c Release -o /app/publish

FROM nginx:alpine AS final
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY nginx-spa.conf /etc/nginx/conf.d/default.conf

EXPOSE 8080
