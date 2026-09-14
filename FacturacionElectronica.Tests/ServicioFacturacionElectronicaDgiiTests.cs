using FacturacionElectronicaDGII.Modelos;

namespace FacturacionElectronicaDGII.Tests;

public class ServicioFacturacionElectronicaDgiiTests
{
    private static ComprobanteDto ComprobanteValido() => new()
    {
        TipoNcf = "32",
        NumeroNcf = "E320001170280",
        FechaEmision = DateTime.UtcNow,
        Emisor = new EmisorDto { Rnc = "130123456", RazonSocial = "SaborByte SRL" },
        Detalle = [new LineaComprobanteDto { Descripcion = "Pastel", Cantidad = 1, PrecioUnitario = 100m, TasaItbis = 0.18m, Impuesto = 18m, Total = 118m }],
        Subtotal = 100m,
        MontoImpuestos = 18m,
        Total = 118m
    };

    private static ServicioFacturacionElectronicaDgii CrearServicio(
        FacturacionElectronicaOpciones? opciones = null, HttpMessageHandlerDePrueba? handler = null) =>
        new(opciones ?? new FacturacionElectronicaOpciones(), new HttpClient(handler ?? new HttpMessageHandlerDePrueba(_ =>
            throw new InvalidOperationException("Esta prueba no debería llamar a la red."))));

    [Fact]
    public void ValidarComprobante_delega_en_ValidadorComprobante()
    {
        var servicio = CrearServicio();

        var resultado = servicio.ValidarComprobante(ComprobanteValido());

        Assert.True(resultado.EsValido);
    }

    [Fact]
    public void GenerarComprobanteXml_rechaza_tipos_de_ecf_distintos_a_32()
    {
        var servicio = CrearServicio();
        var comprobante = ComprobanteValido();
        comprobante.TipoNcf = "31"; // Crédito Fiscal — no implementado todavía

        var excepcion = Assert.Throws<NotSupportedException>(() => servicio.GenerarComprobanteXml(comprobante));
        Assert.Contains("tipo 32", excepcion.Message);
    }

    [Fact]
    public void GenerarComprobanteXml_tipo_32_produce_el_mismo_resultado_que_el_generador_directo()
    {
        var servicio = CrearServicio();
        var comprobante = ComprobanteValido();

        var xml = servicio.GenerarComprobanteXml(comprobante);

        Assert.Equal(GeneradorXmlEcf32.Generar(comprobante), xml);
    }

    [Fact]
    public async Task FirmarComprobanteAsync_falla_con_un_mensaje_claro_si_no_hay_certificado_configurado()
    {
        // Refleja el estado actual real del sistema: RutaCertificado vacío en
        // appsettings hasta que el negocio configure el certificado homologado de DGII.
        var servicio = CrearServicio(new FacturacionElectronicaOpciones { RutaCertificado = null });

        var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
            () => servicio.FirmarComprobanteAsync(ComprobanteValido()));

        Assert.Contains("RutaCertificado", excepcion.Message);
    }

    [Fact]
    public async Task EnviarADgiiAsync_interpreta_estado_cero_como_EnProceso()
    {
        // El flujo completo (semilla -> validación -> recepción) queda cubierto acá con
        // certificado de prueba y respuestas fijas — nada de esto toca la red real de DGII.
        var certificadoPath = System.IO.Path.GetTempFileName();
        var certificado = CertificadoDePrueba.GenerarConClavePrivada();
        System.IO.File.WriteAllBytes(certificadoPath, certificado.Export(System.Security.Cryptography.X509Certificates.X509ContentType.Pfx));

        try
        {
            var opciones = new FacturacionElectronicaOpciones
            {
                RutaCertificado = certificadoPath,
                PasswordCertificado = null,
                UrlSemilla = "https://dgii.local/semilla",
                UrlValidacionCertificado = "https://dgii.local/validar",
                UrlRecepcionEcf = "https://dgii.local/recepcion"
            };

            var handler = new HttpMessageHandlerDePrueba(request =>
            {
                if (request.Method == HttpMethod.Get)
                    return Task.FromResult(HttpMessageHandlerDePrueba.Ok("<SemillaModel><valor>x</valor></SemillaModel>", "text/xml"));

                if (request.RequestUri!.ToString().Contains("validar"))
                    return Task.FromResult(HttpMessageHandlerDePrueba.Ok("""{"token":"jwt-de-prueba"}"""));

                // POST de recepción del e-CF firmado.
                return Task.FromResult(HttpMessageHandlerDePrueba.Ok(
                    """{"trackId":"TRACK-001","estado":0,"mensaje":"Recibido"}"""));
            });

            var servicio = CrearServicio(opciones, handler);

            var resultado = await servicio.EnviarADgiiAsync("<ECF>ya firmado</ECF>");

            Assert.Equal("TRACK-001", resultado.TrackId);
            Assert.Equal("EnProceso", resultado.Estado);
        }
        finally
        {
            System.IO.File.Delete(certificadoPath);
        }
    }

    [Fact]
    public async Task EnviarADgiiAsync_marca_Rechazado_si_dgii_responde_con_error_http()
    {
        var certificadoPath = System.IO.Path.GetTempFileName();
        var certificado = CertificadoDePrueba.GenerarConClavePrivada();
        System.IO.File.WriteAllBytes(certificadoPath, certificado.Export(System.Security.Cryptography.X509Certificates.X509ContentType.Pfx));

        try
        {
            var opciones = new FacturacionElectronicaOpciones { RutaCertificado = certificadoPath };

            var handler = new HttpMessageHandlerDePrueba(request =>
            {
                if (request.Method == HttpMethod.Get)
                    return Task.FromResult(HttpMessageHandlerDePrueba.Ok("<SemillaModel><valor>x</valor></SemillaModel>", "text/xml"));

                if (request.RequestUri!.ToString().Contains("Autenticacion"))
                    return Task.FromResult(HttpMessageHandlerDePrueba.Ok("""{"token":"jwt-de-prueba"}"""));

                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("XML mal formado")
                });
            });

            var servicio = CrearServicio(opciones, handler);

            var resultado = await servicio.EnviarADgiiAsync("<ECF>invalido</ECF>");

            Assert.Equal("Rechazado", resultado.Estado);
            Assert.Contains("400", resultado.Mensaje);
        }
        finally
        {
            System.IO.File.Delete(certificadoPath);
        }
    }
}
