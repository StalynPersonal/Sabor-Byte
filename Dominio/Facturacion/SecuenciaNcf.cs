namespace SaborByte.Dominio.Facturacion;

public class SecuenciaNcf
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SucursalId { get; set; }

    // Serie del NCF: "E" para comprobantes electrónicos (e-CF); NCF tradicional en
    // papel usa otras series (ej. "B"). Es un campo propio, no un literal fijo,
    // porque una misma sucursal puede manejar ambos esquemas en paralelo.
    public string Serie { get; set; } = "E";

    // Tipo de comprobante SIN la serie — ej. "31" Crédito Fiscal, "32" Consumo,
    // "33" Nota de Débito, "34" Nota de Crédito, "44" Régimen Especial,
    // "45" Gubernamental. Confirmado contra ejemplos reales de e-CF firmados
    // (campo <TipoeCF> del XML, que tampoco lleva la serie).
    public required string TipoComprobante { get; set; }
    public long SecuenciaInicial { get; set; }
    public long SecuenciaProxima { get; set; }
    public long SecuenciaFinal { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public bool Activa { get; set; } = true;

    // eNCF real: Serie + Tipo (2 dígitos) + Secuencia (10 dígitos) = 13 caracteres.
    // Ej.: Serie "E" + TipoComprobante "32" + secuencia 1170280 -> "E320001170280".
    public string FormatearNumero(long secuencia) => $"{Serie}{TipoComprobante}{secuencia:D10}";
}
