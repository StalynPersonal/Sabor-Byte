using System.Net.Http.Headers;
using System.Net.Http.Json;
using SaborByte.Web.Api.Dtos;

namespace SaborByte.Web.Api;

public class SaborByteApiClient(HttpClient http, SesionCliente sesion)
{
    private void AdjuntarToken()
    {
        http.DefaultRequestHeaders.Authorization = sesion.Token is null
            ? null
            : new AuthenticationHeaderValue("Bearer", sesion.Token);
    }

    public async Task<LoginResponseDto?> LoginAsync(string nombreUsuario, string password)
    {
        var respuesta = await http.PostAsJsonAsync("api/auth/login",
            new LoginRequestDto { NombreUsuario = nombreUsuario, Password = password });

        if (!respuesta.IsSuccessStatusCode)
            return null;

        var resultado = await respuesta.Content.ReadFromJsonAsync<LoginResponseDto>();
        if (resultado is not null)
            sesion.EstablecerSesion(resultado.Token, resultado.Nombre, resultado.Roles, resultado.SucursalesPermitidas);

        return resultado;
    }

    public async Task<List<ProductoResumenDto>> BuscarProductosAsync(Guid sucursalId, string? texto)
    {
        AdjuntarToken();
        var url = $"api/productos?sucursalId={sucursalId}&texto={Uri.EscapeDataString(texto ?? string.Empty)}";
        return await http.GetFromJsonAsync<List<ProductoResumenDto>>(url) ?? [];
    }

    public async Task<List<CajaResumenDto>> ListarCajasAsync(Guid sucursalId)
    {
        AdjuntarToken();
        return await http.GetFromJsonAsync<List<CajaResumenDto>>($"api/caja?sucursalId={sucursalId}") ?? [];
    }

    public async Task<(bool Exito, Guid? TurnoCajaId, string? Error)> AbrirTurnoAsync(Guid cajaId, decimal montoApertura)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync("api/caja/turnos/abrir",
            new AbrirTurnoRequestDto { CajaId = cajaId, MontoAperturaEfectivo = montoApertura });

        if (!respuesta.IsSuccessStatusCode)
            return (false, null, await LeerMensajeErrorAsync(respuesta));

        var resultado = await respuesta.Content.ReadFromJsonAsync<AbrirTurnoResponseDto>();
        return (true, resultado?.TurnoCajaId, null);
    }

    public async Task<ResumenTurnoDto?> ObtenerResumenTurnoAsync(Guid turnoCajaId)
    {
        AdjuntarToken();
        return await http.GetFromJsonAsync<ResumenTurnoDto>($"api/caja/turnos/{turnoCajaId}/resumen");
    }

    public async Task<(bool Exito, string? Error)> CerrarTurnoAsync(CerrarTurnoRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync("api/caja/turnos/cerrar", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<(bool Exito, VentaResultadoDto? Resultado, string? Error)> CrearVentaAsync(
        Guid sucursalId, CrearVentaRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync($"api/ventas?sucursalId={sucursalId}", request);

        if (!respuesta.IsSuccessStatusCode)
            return (false, null, await LeerMensajeErrorAsync(respuesta));

        return (true, await respuesta.Content.ReadFromJsonAsync<VentaResultadoDto>(), null);
    }

    public async Task<List<ComandaDto>> ObtenerComandasAbiertasAsync(Guid sucursalId)
    {
        AdjuntarToken();
        return await http.GetFromJsonAsync<List<ComandaDto>>($"api/comandas?sucursalId={sucursalId}") ?? [];
    }

    public async Task<(bool Exito, ComandaDto? Comanda, string? Error)> CrearComandaAsync(
        Guid sucursalId, CrearComandaRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync($"api/comandas?sucursalId={sucursalId}", request);

        if (!respuesta.IsSuccessStatusCode)
            return (false, null, await LeerMensajeErrorAsync(respuesta));

        return (true, await respuesta.Content.ReadFromJsonAsync<ComandaDto>(), null);
    }

    public async Task<(bool Exito, string? Error)> CambiarEstadoItemAsync(
        Guid sucursalId, Guid comandaItemId, EstadoItemComanda nuevoEstado)
    {
        AdjuntarToken();
        var respuesta = await http.PutAsJsonAsync(
            $"api/comandas/items/{comandaItemId}/estado?sucursalId={sucursalId}",
            new CambiarEstadoItemRequestDto { NuevoEstado = nuevoEstado });

        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<(bool Exito, string? Error)> CancelarItemAsync(
        Guid sucursalId, Guid comandaItemId, string motivo, RolQueCancelo rol)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync(
            $"api/comandas/items/{comandaItemId}/cancelar?sucursalId={sucursalId}",
            new CancelarItemRequestDto { Motivo = motivo, Rol = rol });

        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<(bool Exito, Guid? Codigo, string? Error)> SolicitarAutorizacionAsync(
        string nombreUsuario, string password, string accion)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync("api/autorizaciones",
            new SolicitarAutorizacionRequestDto { NombreUsuario = nombreUsuario, Password = password, Accion = accion });

        if (!respuesta.IsSuccessStatusCode)
            return (false, null, await LeerMensajeErrorAsync(respuesta));

        var resultado = await respuesta.Content.ReadFromJsonAsync<SolicitarAutorizacionResponseDto>();
        return (true, resultado?.CodigoAutorizacion, null);
    }

    public async Task<List<ProductoDetalleDto>> ListarProductosAsync(Guid sucursalId, bool incluirInactivos = false)
    {
        AdjuntarToken();
        return await http.GetFromJsonAsync<List<ProductoDetalleDto>>(
            $"api/productos/todos?sucursalId={sucursalId}&incluirInactivos={incluirInactivos}") ?? [];
    }

    public async Task<ProductoDetalleDto?> ObtenerProductoAsync(Guid productoId)
    {
        AdjuntarToken();
        return await http.GetFromJsonAsync<ProductoDetalleDto>($"api/productos/{productoId}");
    }

    public async Task<(bool Exito, string? Error)> CrearProductoAsync(Guid sucursalId, GuardarProductoRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync($"api/productos?sucursalId={sucursalId}", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<(bool Exito, string? Error)> ActualizarProductoAsync(Guid productoId, GuardarProductoRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PutAsJsonAsync($"api/productos/{productoId}", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<(bool Exito, string? Error)> DesactivarProductoAsync(Guid productoId)
    {
        AdjuntarToken();
        var respuesta = await http.DeleteAsync($"api/productos/{productoId}");
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<List<CategoriaDto>> ListarCategoriasAsync(Guid sucursalId)
    {
        AdjuntarToken();
        return await http.GetFromJsonAsync<List<CategoriaDto>>($"api/categorias?sucursalId={sucursalId}") ?? [];
    }

    public async Task<(bool Exito, string? Error)> CrearCategoriaAsync(Guid sucursalId, GuardarCategoriaRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync($"api/categorias?sucursalId={sucursalId}", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<List<UsuarioDto>> ListarUsuariosAsync()
    {
        AdjuntarToken();
        return await http.GetFromJsonAsync<List<UsuarioDto>>("api/usuarios") ?? [];
    }

    public async Task<(bool Exito, string? Error)> CrearUsuarioAsync(CrearUsuarioRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync("api/usuarios", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<(bool Exito, string? Error)> DesactivarUsuarioAsync(Guid usuarioId)
    {
        AdjuntarToken();
        var respuesta = await http.DeleteAsync($"api/usuarios/{usuarioId}");
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<SucursalDto?> ObtenerSucursalAsync(Guid sucursalId)
    {
        AdjuntarToken();
        return await http.GetFromJsonAsync<SucursalDto>($"api/sucursales/{sucursalId}");
    }

    public async Task<(bool Exito, string? Error)> ActualizarSucursalAsync(Guid sucursalId, ActualizarSucursalRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PutAsJsonAsync($"api/sucursales/{sucursalId}", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    private static async Task<string?> LeerMensajeErrorAsync(HttpResponseMessage respuesta)
    {
        try
        {
            var cuerpo = await respuesta.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            return cuerpo?.GetValueOrDefault("mensaje") ?? $"Error {(int)respuesta.StatusCode}";
        }
        catch
        {
            return $"Error {(int)respuesta.StatusCode}";
        }
    }
}
