namespace SaborByte.Web.Api.Dtos;

public class ResultadoPaginadoDto<T>
{
    public List<T> Items { get; set; } = [];
    public int Pagina { get; set; }
    public int TamanoPagina { get; set; }
    public int TotalRegistros { get; set; }
    public int TotalPaginas { get; set; }
}
