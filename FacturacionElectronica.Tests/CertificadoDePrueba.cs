using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace FacturacionElectronicaDGII.Tests;

// Certificado autofirmado generado en memoria — sirve para probar que la firma XML-DSig
// funciona correctamente (formato, estructura, KeyInfo) sin necesitar el certificado
// homologado real de DGII, que la empresa todavía no ha configurado (ver
// ServicioFacturacionElectronicaDgii.CargarCertificado). No sirve para un envío real a
// DGII — DGII exige uno emitido por una autoridad certificadora que ellos reconozcan.
public static class CertificadoDePrueba
{
    public static X509Certificate2 GenerarConClavePrivada()
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(
            "CN=SaborByte Pruebas", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        var certificado = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));

        // Reimportar como Pkcs12 y volver a cargar: un X509Certificate2 recién creado con
        // CreateSelfSigned no siempre expone la clave privada de forma utilizable por
        // SignedXml en todas las plataformas — exportar/reimportar lo normaliza.
        var bytes = certificado.Export(X509ContentType.Pfx);
        return X509CertificateLoader.LoadPkcs12(bytes, password: null, X509KeyStorageFlags.Exportable);
    }

    public static X509Certificate2 GenerarSoloPublico() =>
        X509CertificateLoader.LoadCertificate(GenerarConClavePrivada().Export(X509ContentType.Cert));
}
