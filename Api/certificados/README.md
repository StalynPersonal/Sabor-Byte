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
  "PasswordCertificado": "la-contraseña-del-certificado",
  "UrlBaseDgii": ""
}
```

`RutaCertificado` puede ser relativa (a la carpeta de ejecución de la Api,
normalmente `Api/`) o absoluta.

## Producción

En producción, **no** uses `appsettings.json` en texto plano para la
contraseña del certificado — usa un vault de secretos (Azure Key Vault,
variables de entorno inyectadas por el orquestador, etc.) y monta el
archivo del certificado como secreto/volumen, no como archivo del
repositorio desplegado.
