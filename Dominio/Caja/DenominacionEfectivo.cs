namespace SaborByte.Dominio.Caja;

// Catálogo global (no por sucursal) — Admin lo administra desde Central; el cierre de
// turno en Caja solo lo lee para armar el desglose de billetes/monedas (antes era una
// lista fija en código: 2000, 1000, 500, 200, 100, 50, 25, 10, 5, 1).
public class DenominacionEfectivo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Valor { get; set; } // ej. 2000 = billete/moneda de RD$2000
    public bool Activo { get; set; } = true;
}
