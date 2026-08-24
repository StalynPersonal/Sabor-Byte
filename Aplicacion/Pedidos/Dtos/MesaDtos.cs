using SaborByte.Dominio.Pedidos;

namespace SaborByte.Aplicacion.Pedidos.Dtos;

public class MesaDto
{
    public Guid Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public string? Salon { get; set; }
    public int Capacidad { get; set; }
    public EstadoMesa Estado { get; set; }
    public bool Activo { get; set; }
}

public class GuardarMesaRequestDto
{
    public required string Numero { get; set; }
    public string? Salon { get; set; }
    public int Capacidad { get; set; }
}
