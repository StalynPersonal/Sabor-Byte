namespace SaborByte.Dominio.Caja;

public class Caja
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SucursalId { get; set; }
    public required string Numero { get; set; }
    public bool Activa { get; set; } = true;

    // Seguridad: la caja solo puede abrirse desde esta máquina física.
    public string? IpPermitida { get; set; }
    public string? HostnamePermitido { get; set; }
}
