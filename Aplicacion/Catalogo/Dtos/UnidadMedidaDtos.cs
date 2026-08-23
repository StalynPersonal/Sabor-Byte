namespace SaborByte.Aplicacion.Catalogo.Dtos;

public class UnidadMedidaDto
{
    public Guid Id { get; set; }
    public required string Nombre { get; set; }
    public bool Activo { get; set; }
}

public class GuardarUnidadMedidaRequestDto
{
    public required string Nombre { get; set; }
    public bool Activo { get; set; } = true;
}
