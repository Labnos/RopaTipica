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

// ====================== SPA (VITE) ======================
app.UseSpa(spa =>
{
    spa.Options.SourcePath = "ClientApp";

    if (app.Environment.IsDevelopment())
    {
        var clientAppPath = Path.Combine(Directory.GetCurrentDirectory(), "ClientApp");
        var npmPath = "npm";

        // Solo ejecuta npm si está instalado en el sistema
        bool npmExists = System.IO.File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "nodejs", "npm.cmd"))
                      || System.IO.File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "nodejs", "npm.cmd"))
                      || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PATH")?.Split(';').FirstOrDefault(p => p.Contains("nodejs")));

        if (npmExists)
        {
            try
            {
                Console.WriteLine("🚀 Iniciando Vite dev server...");

                var viteProcess = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = npmPath,
                        Arguments = "run dev",
                        WorkingDirectory = clientAppPath,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                viteProcess.OutputDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                        Console.WriteLine($"[Vite] {args.Data}");
                };

                viteProcess.ErrorDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                        Console.WriteLine($"❌ [Vite Error] {args.Data}");
                };

                viteProcess.Start();
                viteProcess.BeginOutputReadLine();
                viteProcess.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ No se pudo iniciar el servidor de Vite automáticamente: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("⚠️ npm no está instalado o no se encuentra en PATH. Ejecuta manualmente 'npm run dev' en la carpeta ClientApp.");
        }

        // Proxy hacia el servidor de desarrollo de Vite
        spa.UseProxyToSpaDevelopmentServer("http://localhost:5173");
    }
    else
    {
        // En producción, sirve los archivos compilados desde wwwroot
        spa.Options.DefaultPageStaticFileOptions = new StaticFileOptions
        {
            OnPrepareResponse = ctx =>
            {
                ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store");
            }
        };
    }
});

// ====================== EJECUCIÓN ======================
app.Run();
