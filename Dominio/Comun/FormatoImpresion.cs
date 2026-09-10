namespace SaborByte.Dominio.Comun;

// Compartido entre Caja (formato de facturas/notas) y Sucursal (formato del recibo
// informativo de deliveries, que no está atado a una caja específica).
public enum FormatoImpresion
{
    Ticket80mm,
    Ticket58mm,
    Carta
}
