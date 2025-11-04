using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using InventarioRopaTipica.Data;
using InventarioRopaTipica.Middleware;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

// ====================== CONFIGURACIÓN DE SERVICIOS ======================
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// ---------- Conexión a base de datos ----------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

try
{
    using var conn = new MySqlConnection(connectionString);
    conn.Open();
    Console.WriteLine("✅ Conexión exitosa al servidor MySQL");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error de conexión: {ex.Message}");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.Parse("8.0.30-mysql"))
        .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
        .EnableDetailedErrors(builder.Environment.IsDevelopment())
);

// ---------- Configuración JWT ----------
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// ---------- Configuración CORS ----------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ---------- Servicios personalizados ----------
builder.Services.AddScoped<InventarioRopaTipica.Helpers.JwtHelper>();
builder.Services.AddScoped<InventarioRopaTipica.Services.IAuthService, InventarioRopaTipica.Services.AuthService>();
builder.Services.AddScoped<InventarioRopaTipica.Services.IUserService, InventarioRopaTipica.Services.UserService>();
builder.Services.AddScoped<InventarioRopaTipica.Services.IProductoService, InventarioRopaTipica.Services.ProductoService>();
builder.Services.AddScoped<InventarioRopaTipica.Services.IVentaService, InventarioRopaTipica.Services.VentaService>();
builder.Services.AddScoped<InventarioRopaTipica.Services.IClienteService, InventarioRopaTipica.Services.ClienteService>();
builder.Services.AddScoped<InventarioRopaTipica.Services.IProveedorService, InventarioRopaTipica.Services.ProveedorService>();

// ---------- Archivos estáticos SPA ----------
builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "wwwroot";
});

// ====================== CONSTRUCCIÓN DEL APP ======================
var app = builder.Build();

// ---------- Pipeline HTTP ----------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// app.UseHttpsRedirection(); // ⚠️ Desactivar en desarrollo si causa redirecciones no deseadas

app.UseStaticFiles();
app.UseSpaStaticFiles();

app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseTokenValidation();
app.UseAuthorization();

app.MapControllerRoute(
    name: "api",
    pattern: "api/{controller}/{action}/{id?}"
);

// ====================== SPA (solo para navegador) ======================
app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api"), spaApp =>
{
    spaApp.UseSpa(spa =>
    {
        spa.Options.SourcePath = "ClientApp";

        if (app.Environment.IsDevelopment())
        {
            try
            {
                using var client = new HttpClient();
                var response = client.GetAsync("http://localhost:5173").Result;
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("🚀 Proxy activo hacia Vite (localhost:5173)");
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:5173");
                }
                else
                {
                    Console.WriteLine("⚠️ Vite no está corriendo. Solo se sirve backend.");
                }
            }
            catch
            {
                Console.WriteLine("⚠️ No se pudo conectar con Vite. Solo se sirve backend.");
            }
        }
        else
        {
            spa.Options.DefaultPageStaticFileOptions = new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store");
                }
            };
        }
    });
});

// ====================== EJECUCIÓN ======================
app.Run();
