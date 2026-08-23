namespace SaborByte.Aplicacion.Caja.Dtos;

public class PorcentajePropinaDto
{
    public Guid Id { get; set; }
    public decimal Valor { get; set; }
    public bool Activo { get; set; }
}

public class GuardarPorcentajePropinaRequestDto
{
    public decimal Valor { get; set; }
    public bool Activo { get; set; } = true;
}

public class DenominacionEfectivoDto
{
    public Guid Id { get; set; }
    public int Valor { get; set; }
    public bool Activo { get; set; }
}

public class GuardarDenominacionEfectivoRequestDto
{
    public int Valor { get; set; }
    public bool Activo { get; set; } = true;
}
