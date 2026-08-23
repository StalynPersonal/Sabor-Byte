namespace SaborByte.Dominio.Caja;

// Catálogo global (no por sucursal) — Admin lo administra desde Central; Caja solo lo
// lee para poblar el selector de propina al facturar (antes era una lista fija en código).
public class PorcentajePropina
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public decimal Valor { get; set; } // ej. 10 = 10%
    public bool Activo { get; set; } = true;
}
