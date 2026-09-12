namespace SaborByte.Web.Caja.Services;

// Puente simple entre el ícono "Cerrar turno" del MudAppBar (MainLayout) y la lógica de
// cierre que vive en Home.razor — MainLayout no tiene acceso directo al estado de la página.
public class CierreTurnoSignal
{
    public event Action? Solicitado;

    public void Solicitar() => Solicitado?.Invoke();
}
