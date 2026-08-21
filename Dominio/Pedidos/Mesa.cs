namespace SaborByte.Dominio.Pedidos;

public enum EstadoMesa
{
    Libre,
    Ocupada
}

public class Mesa
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SucursalId { get; set; }
    public required string Numero { get; set; }
    public string? Salon { get; set; }
    public int Capacidad { get; set; }
    public EstadoMesa Estado { get; set; } = EstadoMesa.Libre;
}
