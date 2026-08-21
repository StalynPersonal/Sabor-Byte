namespace FacturacionElectronicaDGII.Validacion;

public class ResultadoValidacion
{
    public bool EsValido => Errores.Count == 0;
    public List<string> Errores { get; } = [];

    public void AgregarError(string mensaje) => Errores.Add(mensaje);
}
