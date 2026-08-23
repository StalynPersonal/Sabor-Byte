namespace SaborByte.Dominio.Catalogo;

// Catálogo global (no por sucursal) — reemplaza el enum fijo FormaPago que existía en
// Caja y el texto libre que existía en CxC/CxP: Admin puede agregar/desactivar formas
// de pago (ej. "Cheque", "Pago móvil") sin tocar código.
public class MetodoPago
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Nombre { get; set; }

    // Para el desglose de "efectivo — billetes y monedas" del cierre de turno: solo
    // aplica cuando el método es efectivo (antes era el caso especial FormaPago.Efectivo).
    public bool EsEfectivo { get; set; }

    // Para pedir el número de comprobante/autorización al cobrar (antes era el caso
    // especial FormaPago.Tarjeta en FacturaPago).
    public bool RequiereComprobante { get; set; }

    public bool Activo { get; set; } = true;
}
