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

    // Permite ocultar la mesa del módulo Mesero (ej. se retiró del salón) sin borrarla
    // ni perder su historial de comandas/facturas asociadas.
    public bool Activo { get; set; } = true;
}
