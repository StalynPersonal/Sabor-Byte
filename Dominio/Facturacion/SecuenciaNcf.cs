namespace SaborByte.Dominio.Facturacion;

public class SecuenciaNcf
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SucursalId { get; set; }

    public required string TipoComprobante { get; set; } // ej. E31, E32, B01, B02...
    public long SecuenciaInicial { get; set; }
    public long SecuenciaProxima { get; set; }
    public long SecuenciaFinal { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public bool Activa { get; set; } = true;

    public string FormatearNumero(long secuencia) => $"{TipoComprobante}{secuencia:D10}";
}
