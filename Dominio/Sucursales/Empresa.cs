namespace SaborByte.Dominio.Sucursales;

// Dueña de todas las Sucursales — sistema multisucursal, NO multiempresa: solo existe
// una fila de Empresa en toda la base de datos (sembrada una única vez). Sucursal.Nombre
// sigue siendo el nombre de cada local individual; Empresa.Nombre es el que se muestra
// en el AppBar/login de las apps, compartido por todas las sucursales.
public class Empresa
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Nombre { get; set; }
    public string? Rnc { get; set; }
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}
