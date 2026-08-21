# Certificado digital DGII

Coloca aquí el certificado digital (.p12 o .pfx) que la Cámara de Comercio
(u otra autoridad certificadora autorizada) emitió para la firma de
comprobantes electrónicos (e-CF) ante la DGII.

Esta carpeta está excluida de git (ver `.gitignore` en la raíz del repo) —
el certificado y su contraseña NUNCA deben subirse al repositorio.

## Configuración

En `Api/appsettings.Development.json`, sección `FacturacionElectronica`:

```json
"FacturacionElectronica": {
  "RutaCertificado": "certificados/mi-certificado.p12",
  "PasswordCertificado": "la-contraseña-del-certificado"
}
```

`RutaCertificado` puede ser relativa (a la carpeta de ejecución de la Api,
normalmente `Api/`) o absoluta.

## URLs de los servicios de DGII

Las URLs del ambiente de pruebas "TesteCF" ya vienen por defecto en
`FacturacionElectronicaOpciones` (no hace falta configurarlas para probar
contra TesteCF). Para producción, DGII entrega un set de URLs equivalente
— sobreescríbelas en `appsettings.Production.json` (o el mecanismo de config
que uses en el servidor), agregando esta misma sección `FacturacionElectronica`
con `UrlSemilla`, `UrlValidacionCertificado`, `UrlRecepcionEcf` y
`UrlConsultaTrackId`. Nunca cambies el valor por defecto en el código para
apuntar a producción — eso se decide por ambiente, vía configuración.

## Producción

En producción, **no** uses `appsettings.json` en texto plano para la
contraseña del certificado — usa un vault de secretos (Azure Key Vault,
variables de entorno inyectadas por el orquestador, etc.) y monta el
archivo del certificado como secreto/volumen, no como archivo del
repositorio desplegado.
