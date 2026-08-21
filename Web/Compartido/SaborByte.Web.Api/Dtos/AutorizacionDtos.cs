namespace SaborByte.Web.Api.Dtos;

public class SolicitarAutorizacionRequestDto
{
    public string NombreUsuario { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
}

public class SolicitarAutorizacionResponseDto
{
    public Guid CodigoAutorizacion { get; set; }
    public DateTime Expira { get; set; }
}
