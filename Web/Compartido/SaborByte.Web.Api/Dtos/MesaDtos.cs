namespace SaborByte.Web.Api.Dtos;

public enum EstadoMesa
{
    Libre,
    Ocupada
}

public class MesaDto
{
    public Guid Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public string? Salon { get; set; }
    public int Capacidad { get; set; }
    public EstadoMesa Estado { get; set; }
}

public class GuardarMesaRequestDto
{
    public string Numero { get; set; } = string.Empty;
    public string? Salon { get; set; }
    public int Capacidad { get; set; }
}
