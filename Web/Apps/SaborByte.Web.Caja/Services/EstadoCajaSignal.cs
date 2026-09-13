namespace SaborByte.Web.Caja.Services;

// Puente simple entre Home.razor (donde vive el estado de "turno bloqueado de otro
// día") y el MudAppBar de MainLayout — mismo criterio que CierreTurnoSignal, pero en
// sentido inverso: aquí Home.razor informa, MainLayout reacciona.
public class EstadoCajaSignal
{
    public bool TurnoBloqueado { get; private set; }

    public event Action? Cambiado;

    public void EstablecerTurnoBloqueado(bool bloqueado)
    {
        if (TurnoBloqueado == bloqueado) return;
        TurnoBloqueado = bloqueado;
        Cambiado?.Invoke();
    }
}
