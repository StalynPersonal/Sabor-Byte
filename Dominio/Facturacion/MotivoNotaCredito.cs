namespace SaborByte.Dominio.Facturacion;

// Catálogo global (no por sucursal, igual que Rol) de motivos seleccionables al emitir
// una nota de crédito/débito — reemplaza el texto libre que no dejaba reportar ni
// estandarizar por qué se estaban emitiendo notas.
public class MotivoNotaCredito
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Nombre { get; set; }
    public bool Activo { get; set; } = true;
}
