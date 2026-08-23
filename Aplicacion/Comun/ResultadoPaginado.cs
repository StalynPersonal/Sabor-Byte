namespace SaborByte.Aplicacion.Comun;

public class ResultadoPaginado<T>
{
    public List<T> Items { get; set; } = [];
    public int Pagina { get; set; }
    public int TamanoPagina { get; set; }
    public int TotalRegistros { get; set; }
    public int TotalPaginas => TamanoPagina <= 0 ? 0 : (int)Math.Ceiling(TotalRegistros / (double)TamanoPagina);
}
