using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SaborByte.Api.Hubs;
using SaborByte.Api.Salud;
using SaborByte.Aplicacion.Identidad;
using SaborByte.Aplicacion.Interfaces;
using SaborByte.Infraestructura;
using SaborByte.Infraestructura.Persistencia;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AgregarInfraestructura(builder.Configuration);

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Falta configurar Jwt:Key en appsettings.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };

        // SignalR (WebSockets) no puede mandar el header Authorization en el handshake
        // del navegador; el cliente manda el JWT como query string "access_token".
        opciones.Events = new JwtBearerEvents
        {
            OnMessageReceived = contexto =>
            {
                var token = contexto.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token) && contexto.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                    contexto.Token = token;

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSignalR();
builder.Services.AddScoped<INotificadorComandas, NotificadorComandasSignalR>();

// Las 4 apps Blazor viven en orígenes distintos al de la Api; se autentican con JWT
// (no cookies), así que restringir a estos orígenes es defensa en profundidad, no lo
// único que evita CSRF. En desarrollo se permite cualquier origen para no depender de
// tener las 4 apps corriendo en puertos fijos conocidos.
var origenesPermitidos = new[]
{
    "https://wonderful-grass-0dd2dd60f.7.azurestaticapps.net", // Mesero
    "https://zealous-moss-0a8da5e0f.7.azurestaticapps.net",    // Cocina
    "https://white-bush-01696d50f.7.azurestaticapps.net",      // Caja
    "https://orange-hill-00088080f.7.azurestaticapps.net"      // Central
};

builder.Services.AddCors(opciones =>
{
    opciones.AddPolicy("AppsBlazor", politica =>
    {
        if (builder.Environment.IsDevelopment())
            politica.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        else
            politica.WithOrigins(origenesPermitidos).AllowAnyHeader().AllowAnyMethod();
    });
});

// Rate limiting nativo de .NET: política estricta para login, más permisiva para el resto.
builder.Services.AddRateLimiter(opciones =>
{
    opciones.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    opciones.AddFixedWindowLimiter("login", limite =>
    {
        limite.Window = TimeSpan.FromMinutes(1);
        limite.PermitLimit = 5;
        limite.QueueLimit = 0;
    });

    opciones.AddFixedWindowLimiter("sincronizacion", limite =>
    {
        limite.Window = TimeSpan.FromSeconds(10);
        limite.PermitLimit = 30;
        limite.QueueLimit = 5;
        limite.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    opciones.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(contexto =>
        RateLimitPartition.GetFixedWindowLimiter(
            contexto.Connection.RemoteIpAddress?.ToString() ?? "sin-ip",
            _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromSeconds(10),
                PermitLimit = 60,
                QueueLimit = 0
            }));
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<SaborByteDbContext>(name: "base-de-datos")
    .AddCheck<FacturacionPendienteHealthCheck>("facturacion-electronica-pendiente");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<SaborByteDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await db.Database.MigrateAsync();
    await SeedData.EjecutarAsync(db, passwordHasher);
    await SeedData.SeedCatalogoDemoAsync(db);
    await SeedData.SeedConfiguracionCajaAsync(db);
    await SeedData.SeedCxcCxpDemoAsync(db);
    await SeedData.RepararHistorialPagosCxcCxpAsync(db);
}

app.UseHttpsRedirection();

app.UseCors("AppsBlazor");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// Heartbeat de presencia: en cada request autenticado, refresca FechaUltimaActividad de la
// sesión (con throttling de 60s dentro de RegistrarActividadAsync) para que "usuarios activos
// ahora" en Central refleje uso real y no solo el instante del login.
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var sesionClaim = context.User.FindFirst("sesion")?.Value;
        if (Guid.TryParse(sesionClaim, out var sesionActivaId))
        {
            var autenticacion = context.RequestServices.GetRequiredService<AutenticacionAppService>();
            await autenticacion.RegistrarActividadAsync(sesionActivaId, context.RequestAborted);
        }
    }

    await next();
});

app.MapControllers();
app.MapHub<ComandaHub>("/hubs/comandas");
app.MapHealthChecks("/health");

app.Run();

// Necesario para que WebApplicationFactory<Program> (pruebas de integración) pueda
// referenciar este ensamblado — con top-level statements la clase Program es interna
// por defecto y no es visible fuera del proyecto sin esta declaración parcial.
public partial class Program;
