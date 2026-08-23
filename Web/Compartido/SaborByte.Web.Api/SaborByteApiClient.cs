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
            sesion.EstablecerSesion(resultado.Token, resultado.Nombre, resultado.Roles,
                resultado.SucursalesPermitidas.Select(s => new SucursalPermitida(s.Id, s.Nombre, s.EmpresaNombre)).ToList());

        return resultado;
    }

    public async Task<(bool Exito, string? Error)> SeleccionarSucursalActivaAsync(Guid sucursalId)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync("api/auth/sesion/sucursal", new SeleccionarSucursalActivaRequestDto { SucursalId = sucursalId });
        if (!respuesta.IsSuccessStatusCode)
            return (false, await LeerMensajeErrorAsync(respuesta));

        sesion.SeleccionarSucursalActiva(sucursalId);
        return (true, null);
    }

    public async Task CerrarSesionAsync()
    {
        AdjuntarToken();
        try { await http.PostAsync("api/auth/logout", null); }
        catch { /* si no hay red, igual se cierra la sesión local */ }
        sesion.CerrarSesion();
    }

    public async Task<(bool Exito, string? Error)> CambiarPasswordAsync(string passwordActual, string passwordNueva)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync("api/auth/cambiar-password",
            new CambiarPasswordRequestDto { PasswordActual = passwordActual, PasswordNueva = passwordNueva });
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<List<SesionActivaDto>> ListarSesionesActivasAsync()
    {
        AdjuntarToken();
        return await http.GetFromJsonAsync<List<SesionActivaDto>>("api/auth/sesiones-activas") ?? [];
    }

    public async Task<List<ProductoResumenDto>> BuscarProductosAsync(string? texto, Guid? categoriaId = null)
    {
        AdjuntarToken();
        var url = $"api/productos?texto={Uri.EscapeDataString(texto ?? string.Empty)}";
        if (categoriaId is not null) url += $"&categoriaId={categoriaId}";
        return await http.GetFromJsonAsync<List<ProductoResumenDto>>(url) ?? [];
    }

    public async Task<List<CajaResumenDto>> ListarCajasAsync(Guid sucursalId)
    {
        AdjuntarToken();
        return await http.GetFromJsonAsync<List<CajaResumenDto>>($"api/caja?sucursalId={sucursalId}") ?? [];
    }

    public async Task<List<CajaDto>> ListarCajasGestionAsync(Guid sucursalId)
    {
        AdjuntarToken();
        return await http.GetFromJsonAsync<List<CajaDto>>($"api/caja/gestion?sucursalId={sucursalId}") ?? [];
    }

    public async Task<(bool Exito, string? Error)> CrearCajaAsync(Guid sucursalId, GuardarCajaRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync($"api/caja/gestion?sucursalId={sucursalId}", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<(bool Exito, string? Error)> ActualizarCajaAsync(Guid sucursalId, Guid cajaId, GuardarCajaRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PutAsJsonAsync($"api/caja/gestion/{cajaId}?sucursalId={sucursalId}", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<TurnoAbiertoDto?> ObtenerTurnoAbiertoAsync(Guid cajaId)
    {
        if (cajaId == Guid.Empty)
            return null;

        AdjuntarToken();
        try
        {
            return await http.GetFromJsonAsync<TurnoAbiertoDto>($"api/caja/turnos/abierto?cajaId={cajaId}");
        }
        catch (Exception ex) when (ex is HttpRequestException or System.Text.Json.JsonException)
        {
            // No dejar que una respuesta inesperada (error del servidor, red caída) tumbe
            // toda la pantalla de Caja al iniciar — se trata como "no hay turno abierto" y
            // el cajero simplemente abre uno nuevo si hace falta.
            return null;
        }
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

    public async Task<(bool Exito, string? Error)> CancelarComandaAsync(
        Guid sucursalId, Guid comandaId, string motivo, RolQueCancelo rol)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync(
            $"api/comandas/{comandaId}/cancelar?sucursalId={sucursalId}",
            new CancelarComandaRequestDto { Motivo = motivo, Rol = rol });

        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<List<MesaDto>> ListarMesasAsync(Guid sucursalId)
    {
        AdjuntarToken();
        return await http.GetFromJsonAsync<List<MesaDto>>($"api/mesas?sucursalId={sucursalId}") ?? [];
    }

    public async Task<(bool Exito, string? Error)> CrearMesaAsync(Guid sucursalId, GuardarMesaRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync($"api/mesas?sucursalId={sucursalId}", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<(bool Exito, string? Error)> ActualizarMesaAsync(Guid sucursalId, Guid mesaId, GuardarMesaRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PutAsJsonAsync($"api/mesas/{mesaId}?sucursalId={sucursalId}", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<(bool Exito, string? Error)> LiberarMesaAsync(Guid sucursalId, Guid mesaId)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsync($"api/mesas/{mesaId}/liberar?sucursalId={sucursalId}", null);
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

    public async Task<ResultadoPaginadoDto<ProductoDetalleDto>> ListarProductosAsync(
        int pagina, int tamanoPagina, string? texto = null, TipoProducto? tipo = null,
        bool incluirInactivos = false, Guid? sucursalId = null)
    {
        AdjuntarToken();
        var url = $"api/productos/todos?pagina={pagina}&tamanoPagina={tamanoPagina}&incluirInactivos={incluirInactivos}";
        if (!string.IsNullOrWhiteSpace(texto))
            url += $"&texto={Uri.EscapeDataString(texto)}";
        if (tipo is not null)
            url += $"&tipo={tipo}";
        if (sucursalId is not null)
            url += $"&sucursalId={sucursalId}";

        return await http.GetFromJsonAsync<ResultadoPaginadoDto<ProductoDetalleDto>>(url)
            ?? new ResultadoPaginadoDto<ProductoDetalleDto>();
    }

    public async Task<List<MovimientoInventarioDto>> ListarMovimientosInventarioAsync(Guid sucursalId, Guid? productoId = null)
    {
        AdjuntarToken();
        var url = $"api/inventario/movimientos?sucursalId={sucursalId}" + (productoId is null ? "" : $"&productoId={productoId}");
        return await http.GetFromJsonAsync<List<MovimientoInventarioDto>>(url) ?? [];
    }

    public async Task<(bool Exito, string? Error)> RegistrarEntradaInventarioAsync(Guid sucursalId, RegistrarEntradaRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync($"api/inventario/entradas?sucursalId={sucursalId}", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<(bool Exito, string? Error)> RegistrarAjusteInventarioAsync(Guid sucursalId, RegistrarAjusteRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync($"api/inventario/ajustes?sucursalId={sucursalId}", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<ProductoDetalleDto?> ObtenerProductoAsync(Guid productoId, Guid? sucursalId = null)
    {
        AdjuntarToken();
        var url = $"api/productos/{productoId}";
        if (sucursalId is not null)
            url += $"?sucursalId={sucursalId}";
        return await http.GetFromJsonAsync<ProductoDetalleDto>(url);
    }

    public async Task<(bool Exito, string? Error)> CrearProductoAsync(GuardarProductoRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync("api/productos", request);
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

    public async Task<(bool Exito, string? Error)> ActivarProductoAsync(Guid productoId)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsync($"api/productos/{productoId}/activar", null);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<List<CategoriaDto>> ListarCategoriasAsync(bool incluirInactivos = false)
    {
        AdjuntarToken();
        return await http.GetFromJsonAsync<List<CategoriaDto>>($"api/categorias?incluirInactivos={incluirInactivos}") ?? [];
    }

    public async Task<(bool Exito, string? Error)> CrearCategoriaAsync(GuardarCategoriaRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync("api/categorias", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<(bool Exito, string? Error)> ActualizarCategoriaAsync(Guid categoriaId, GuardarCategoriaRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PutAsJsonAsync($"api/categorias/{categoriaId}", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<(bool Exito, string? Error)> CrearComboAsync(CrearComboRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync("api/productos/combos", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<(bool Exito, string? Error)> ConfigurarUmbralesInventarioAsync(Guid sucursalId, ConfigurarUmbralesRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PutAsJsonAsync($"api/inventario/umbrales?sucursalId={sucursalId}", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<List<ClienteDto>> BuscarClientesAsync(Guid sucursalId, string? texto = null)
    {
        AdjuntarToken();
        var url = $"api/clientes?sucursalId={sucursalId}&texto={Uri.EscapeDataString(texto ?? string.Empty)}";
        return await http.GetFromJsonAsync<List<ClienteDto>>(url) ?? [];
    }

    public async Task<(bool Exito, Guid? ClienteId, string? Error)> CrearClienteAsync(Guid sucursalId, GuardarClienteRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync($"api/clientes?sucursalId={sucursalId}", request);
        if (!respuesta.IsSuccessStatusCode)
            return (false, null, await LeerMensajeErrorAsync(respuesta));

        var resultado = await respuesta.Content.ReadFromJsonAsync<IdRespuestaDto>();
        return (true, resultado?.Id, null);
    }

    public async Task<(bool Exito, string? Error)> ActualizarClienteAsync(Guid sucursalId, Guid clienteId, GuardarClienteRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PutAsJsonAsync($"api/clientes/{clienteId}?sucursalId={sucursalId}", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<(bool Exito, string? Error)> DesactivarClienteAsync(Guid sucursalId, Guid clienteId)
    {
        AdjuntarToken();
        var respuesta = await http.DeleteAsync($"api/clientes/{clienteId}?sucursalId={sucursalId}");
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<ResultadoPaginadoDto<FacturaResumenDto>> BuscarFacturasAsync(
        Guid sucursalId, int pagina, int tamanoPagina, string? texto = null,
        DateTime? desde = null, DateTime? hasta = null,
        decimal? montoMinimo = null, decimal? montoMaximo = null, Guid? cajaId = null)
    {
        AdjuntarToken();
        var url = $"api/facturas?sucursalId={sucursalId}&pagina={pagina}&tamanoPagina={tamanoPagina}&texto={Uri.EscapeDataString(texto ?? string.Empty)}";
        if (desde is not null) url += $"&desde={desde:O}";
        if (hasta is not null) url += $"&hasta={hasta:O}";
        if (montoMinimo is not null) url += $"&montoMinimo={montoMinimo}";
        if (montoMaximo is not null) url += $"&montoMaximo={montoMaximo}";
        if (cajaId is not null) url += $"&cajaId={cajaId}";
        return await http.GetFromJsonAsync<ResultadoPaginadoDto<FacturaResumenDto>>(url) ?? new();
    }

    public async Task<FacturaDetalleCompletoDto?> ObtenerFacturaDetalleAsync(Guid sucursalId, Guid facturaId)
    {
        AdjuntarToken();
        return await http.GetFromJsonAsync<FacturaDetalleCompletoDto>($"api/facturas/{facturaId}?sucursalId={sucursalId}");
    }

    public async Task<ResultadoPaginadoDto<NotaCreditoDto>> ListarNotasAsync(
        Guid sucursalId, int pagina, int tamanoPagina, string? texto = null,
        DateTime? desde = null, DateTime? hasta = null,
        decimal? montoMinimo = null, decimal? montoMaximo = null, Guid? cajaId = null)
    {
        AdjuntarToken();
        var url = $"api/notascredito?sucursalId={sucursalId}&pagina={pagina}&tamanoPagina={tamanoPagina}&texto={Uri.EscapeDataString(texto ?? string.Empty)}";
        if (desde is not null) url += $"&desde={desde:O}";
        if (hasta is not null) url += $"&hasta={hasta:O}";
        if (montoMinimo is not null) url += $"&montoMinimo={montoMinimo}";
        if (montoMaximo is not null) url += $"&montoMaximo={montoMaximo}";
        if (cajaId is not null) url += $"&cajaId={cajaId}";
        return await http.GetFromJsonAsync<ResultadoPaginadoDto<NotaCreditoDto>>(url) ?? new();
    }

    public async Task<List<NotaCreditoDto>> ListarNotasPorFacturaAsync(Guid sucursalId, Guid facturaId)
    {
        AdjuntarToken();
        return await http.GetFromJsonAsync<List<NotaCreditoDto>>(
            $"api/notascredito/por-factura/{facturaId}?sucursalId={sucursalId}") ?? [];
    }

    public async Task<(bool Exito, NotaCreditoDto? Nota, string? Error)> CrearNotaAsync(Guid sucursalId, CrearNotaRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync($"api/notascredito?sucursalId={sucursalId}", request);
        if (!respuesta.IsSuccessStatusCode)
            return (false, null, await LeerMensajeErrorAsync(respuesta));

        return (true, await respuesta.Content.ReadFromJsonAsync<NotaCreditoDto>(), null);
    }

    public async Task<List<FacturaDetalleDisponibleDto>> ObtenerDetalleDisponibleAsync(Guid sucursalId, Guid facturaId)
    {
        AdjuntarToken();
        return await http.GetFromJsonAsync<List<FacturaDetalleDisponibleDto>>(
            $"api/notascredito/facturas/{facturaId}/detalle?sucursalId={sucursalId}") ?? [];
    }

    public async Task<List<MotivoNotaCreditoDto>> ListarMotivosNotaCreditoAsync(bool incluirInactivos = false)
    {
        AdjuntarToken();
        return await http.GetFromJsonAsync<List<MotivoNotaCreditoDto>>($"api/motivosnotacredito?incluirInactivos={incluirInactivos}") ?? [];
    }

    public async Task<(bool Exito, string? Error)> CrearMotivoNotaCreditoAsync(GuardarMotivoNotaCreditoRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync("api/motivosnotacredito", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<(bool Exito, string? Error)> ActualizarMotivoNotaCreditoAsync(Guid motivoId, GuardarMotivoNotaCreditoRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PutAsJsonAsync($"api/motivosnotacredito/{motivoId}", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<List<MetodoPagoDto>> ListarMetodosPagoAsync(bool incluirInactivos = false)
    {
        AdjuntarToken();
        return await http.GetFromJsonAsync<List<MetodoPagoDto>>($"api/metodospago?incluirInactivos={incluirInactivos}") ?? [];
    }

    public async Task<(bool Exito, string? Error)> CrearMetodoPagoAsync(GuardarMetodoPagoRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync("api/metodospago", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<(bool Exito, string? Error)> ActualizarMetodoPagoAsync(Guid metodoPagoId, GuardarMetodoPagoRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PutAsJsonAsync($"api/metodospago/{metodoPagoId}", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<List<PorcentajePropinaDto>> ListarPorcentajesPropinaAsync(bool incluirInactivos = false)
    {
        AdjuntarToken();
        return await http.GetFromJsonAsync<List<PorcentajePropinaDto>>($"api/configuracioncaja/propinas?incluirInactivos={incluirInactivos}") ?? [];
    }

    public async Task<(bool Exito, string? Error)> CrearPorcentajePropinaAsync(GuardarPorcentajePropinaRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync("api/configuracioncaja/propinas", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<(bool Exito, string? Error)> ActualizarPorcentajePropinaAsync(Guid porcentajeId, GuardarPorcentajePropinaRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PutAsJsonAsync($"api/configuracioncaja/propinas/{porcentajeId}", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<List<DenominacionEfectivoDto>> ListarDenominacionesEfectivoAsync(bool incluirInactivos = false)
    {
        AdjuntarToken();
        return await http.GetFromJsonAsync<List<DenominacionEfectivoDto>>($"api/configuracioncaja/denominaciones?incluirInactivos={incluirInactivos}") ?? [];
    }

    public async Task<(bool Exito, string? Error)> CrearDenominacionEfectivoAsync(GuardarDenominacionEfectivoRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync("api/configuracioncaja/denominaciones", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<(bool Exito, string? Error)> ActualizarDenominacionEfectivoAsync(Guid denominacionId, GuardarDenominacionEfectivoRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PutAsJsonAsync($"api/configuracioncaja/denominaciones/{denominacionId}", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<List<UnidadMedidaDto>> ListarUnidadesMedidaAsync(bool incluirInactivos = false)
    {
        AdjuntarToken();
        return await http.GetFromJsonAsync<List<UnidadMedidaDto>>($"api/unidadesmedida?incluirInactivos={incluirInactivos}") ?? [];
    }

    public async Task<(bool Exito, string? Error)> CrearUnidadMedidaAsync(GuardarUnidadMedidaRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync("api/unidadesmedida", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<(bool Exito, string? Error)> ActualizarUnidadMedidaAsync(Guid unidadMedidaId, GuardarUnidadMedidaRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PutAsJsonAsync($"api/unidadesmedida/{unidadMedidaId}", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<List<ProveedorDto>> ListarProveedoresAsync(Guid sucursalId)
    {
        AdjuntarToken();
        return await http.GetFromJsonAsync<List<ProveedorDto>>($"api/cxccxp/proveedores?sucursalId={sucursalId}") ?? [];
    }

    public async Task<(bool Exito, string? Error)> CrearProveedorAsync(Guid sucursalId, GuardarProveedorRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync($"api/cxccxp/proveedores?sucursalId={sucursalId}", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<List<SecuenciaNcfDto>> ListarSecuenciasNcfAsync(Guid sucursalId)
    {
        AdjuntarToken();
        return await http.GetFromJsonAsync<List<SecuenciaNcfDto>>($"api/secuenciasncf?sucursalId={sucursalId}") ?? [];
    }

    public async Task<(bool Exito, string? Error)> CrearSecuenciaNcfAsync(Guid sucursalId, GuardarSecuenciaNcfRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync($"api/secuenciasncf?sucursalId={sucursalId}", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<(bool Exito, string? Error)> ActualizarSecuenciaNcfAsync(Guid sucursalId, Guid secuenciaId, GuardarSecuenciaNcfRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PutAsJsonAsync($"api/secuenciasncf/{secuenciaId}?sucursalId={sucursalId}", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<ResultadoPaginadoDto<CuentaPorCobrarDto>> ListarPorCobrarAsync(Guid sucursalId, int pagina, int tamanoPagina)
    {
        AdjuntarToken();
        return await http.GetFromJsonAsync<ResultadoPaginadoDto<CuentaPorCobrarDto>>(
            $"api/cxccxp/porcobrar?sucursalId={sucursalId}&pagina={pagina}&tamanoPagina={tamanoPagina}") ?? new();
    }

    public async Task<(bool Exito, string? Error)> CrearCuentaPorCobrarAsync(Guid sucursalId, CrearCuentaPorCobrarRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync($"api/cxccxp/porcobrar?sucursalId={sucursalId}", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<(bool Exito, string? Error)> PagarCuentaPorCobrarAsync(Guid sucursalId, Guid cuentaId, RegistrarPagoRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync($"api/cxccxp/porcobrar/{cuentaId}/pagos?sucursalId={sucursalId}", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<ResultadoPaginadoDto<CuentaPorPagarDto>> ListarPorPagarAsync(Guid sucursalId, int pagina, int tamanoPagina)
    {
        AdjuntarToken();
        return await http.GetFromJsonAsync<ResultadoPaginadoDto<CuentaPorPagarDto>>(
            $"api/cxccxp/porpagar?sucursalId={sucursalId}&pagina={pagina}&tamanoPagina={tamanoPagina}") ?? new();
    }

    public async Task<(bool Exito, string? Error)> CrearCuentaPorPagarAsync(Guid sucursalId, CrearCuentaPorPagarRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync($"api/cxccxp/porpagar?sucursalId={sucursalId}", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<(bool Exito, string? Error)> PagarCuentaPorPagarAsync(Guid sucursalId, Guid cuentaId, RegistrarPagoRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync($"api/cxccxp/porpagar/{cuentaId}/pagos?sucursalId={sucursalId}", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<ReporteVentasConsolidadoDto> VentasPorSucursalAsync(List<Guid> sucursalesIds, DateTime desde, DateTime hasta)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync("api/reportes/ventas-por-sucursal",
            new ReporteVentasRequestDto { SucursalesIds = sucursalesIds, Desde = desde, Hasta = hasta });
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<ReporteVentasConsolidadoDto>() ?? new ReporteVentasConsolidadoDto();
    }

    public async Task<List<VentaPorProductoDto>> VentasPorProductoAsync(Guid sucursalId, RangoFechasRequestDto rango)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync($"api/reportes/ventas-por-producto?sucursalId={sucursalId}", rango);
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<List<VentaPorProductoDto>>() ?? [];
    }

    public async Task<List<VentaPorHoraDto>> VentasPorHoraAsync(Guid sucursalId, RangoFechasRequestDto rango)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync($"api/reportes/ventas-por-hora?sucursalId={sucursalId}", rango);
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<List<VentaPorHoraDto>>() ?? [];
    }

    public async Task<DashboardResumenDto?> ObtenerDashboardAsync(Guid sucursalId)
    {
        AdjuntarToken();
        return await http.GetFromJsonAsync<DashboardResumenDto>($"api/reportes/dashboard?sucursalId={sucursalId}");
    }

    public async Task<List<VentaResumenDiaDto>> VentasResumenPorDiaAsync(Guid sucursalId, RangoFechasRequestDto rango)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync($"api/reportes/ventas-resumen-por-dia?sucursalId={sucursalId}", rango);
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<List<VentaResumenDiaDto>>() ?? [];
    }

    public async Task<List<VentaDetalleDto>> VentasDetalleAsync(Guid sucursalId, RangoFechasRequestDto rango)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync($"api/reportes/ventas-detalle?sucursalId={sucursalId}", rango);
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<List<VentaDetalleDto>>() ?? [];
    }

    public async Task<List<VentaPorCategoriaDto>> VentasPorCategoriaAsync(Guid sucursalId, RangoFechasRequestDto rango)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync($"api/reportes/ventas-por-categoria?sucursalId={sucursalId}", rango);
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<List<VentaPorCategoriaDto>>() ?? [];
    }

    public async Task<List<VentaPorMetodoPagoDto>> VentasPorMetodoPagoAsync(Guid sucursalId, RangoFechasRequestDto rango)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync($"api/reportes/ventas-por-metodo-pago?sucursalId={sucursalId}", rango);
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<List<VentaPorMetodoPagoDto>>() ?? [];
    }

    public async Task<List<MovimientoInventarioReporteDto>> MovimientosInventarioReporteAsync(Guid sucursalId, RangoFechasRequestDto rango)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync($"api/reportes/movimientos-inventario?sucursalId={sucursalId}", rango);
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<List<MovimientoInventarioReporteDto>>() ?? [];
    }

    public async Task<List<CuentaPendienteDto>> CxCPendientesAsync(Guid sucursalId)
    {
        AdjuntarToken();
        return await http.GetFromJsonAsync<List<CuentaPendienteDto>>($"api/reportes/cxc-pendientes?sucursalId={sucursalId}") ?? [];
    }

    public async Task<List<CuentaPendienteDto>> CxPPendientesAsync(Guid sucursalId)
    {
        AdjuntarToken();
        return await http.GetFromJsonAsync<List<CuentaPendienteDto>>($"api/reportes/cxp-pendientes?sucursalId={sucursalId}") ?? [];
    }

    public async Task<ResultadoPaginadoDto<UsuarioDto>> ListarUsuariosAsync(int pagina, int tamanoPagina)
    {
        AdjuntarToken();
        return await http.GetFromJsonAsync<ResultadoPaginadoDto<UsuarioDto>>($"api/usuarios?pagina={pagina}&tamanoPagina={tamanoPagina}") ?? new();
    }

    public async Task<(bool Exito, string? Error)> CrearUsuarioAsync(CrearUsuarioRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync("api/usuarios", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<(bool Exito, string? Error)> ActualizarUsuarioAsync(Guid usuarioId, ActualizarUsuarioRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PutAsJsonAsync($"api/usuarios/{usuarioId}", request);
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

    public async Task<(bool Exito, string? Error)> ActualizarSmtpSucursalAsync(Guid sucursalId, ActualizarSmtpRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PutAsJsonAsync($"api/sucursales/{sucursalId}/smtp", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<List<SucursalResumenDto>> ListarSucursalesGestionAsync()
    {
        AdjuntarToken();
        return await http.GetFromJsonAsync<List<SucursalResumenDto>>("api/sucursales/gestion") ?? [];
    }

    public async Task<(bool Exito, string? Error)> CrearSucursalAsync(CrearSucursalRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PostAsJsonAsync("api/sucursales", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    public async Task<EmpresaDto?> ObtenerEmpresaAsync()
    {
        AdjuntarToken();
        return await http.GetFromJsonAsync<EmpresaDto>("api/empresa");
    }

    public async Task<(bool Exito, string? Error)> ActualizarEmpresaAsync(GuardarEmpresaRequestDto request)
    {
        AdjuntarToken();
        var respuesta = await http.PutAsJsonAsync("api/empresa", request);
        return respuesta.IsSuccessStatusCode ? (true, null) : (false, await LeerMensajeErrorAsync(respuesta));
    }

    private static async Task<string?> LeerMensajeErrorAsync(HttpResponseMessage respuesta)
    {
        try
        {
            using var documento = System.Text.Json.JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
            var raiz = documento.RootElement;

            // Errores de dominio (BadRequest/NotFound con `{ mensaje: "..." }`, ver controllers).
            if (raiz.TryGetProperty("mensaje", out var mensaje) && mensaje.ValueKind == System.Text.Json.JsonValueKind.String)
                return mensaje.GetString();

            // Validación automática de [ApiController] (campos [Required] vacíos, etc.):
            // ValidationProblemDetails con `{ errors: { "Campo": ["mensaje", ...] } }`.
            if (raiz.TryGetProperty("errors", out var errores) && errores.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                var primerMensaje = errores.EnumerateObject()
                    .SelectMany(campo => campo.Value.EnumerateArray().Select(m => m.GetString()))
                    .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m));

                if (primerMensaje is not null)
                    return primerMensaje;
            }

            return $"Error {(int)respuesta.StatusCode}";
        }
        catch
        {
            return $"Error {(int)respuesta.StatusCode}";
        }
    }
}
