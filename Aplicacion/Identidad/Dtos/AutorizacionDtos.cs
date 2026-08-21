namespace SaborByte.Aplicacion.Identidad.Dtos;

public class SolicitarAutorizacionRequestDto
{
    public required string NombreUsuario { get; set; }
    public required string Password { get; set; }
    public required string Accion { get; set; }
}

public class SolicitarAutorizacionResponseDto
{
    public Guid CodigoAutorizacion { get; set; }
    public DateTime Expira { get; set; }
}
