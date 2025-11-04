using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using InventarioRopaTipica.Data;
using InventarioRopaTipica.Middleware;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

// Configuración de servicios
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Base de datos
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

// JWT
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

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Servicios personalizados
builder.Services.AddScoped<InventarioRopaTipica.Helpers.JwtHelper>();
builder.Services.AddScoped<InventarioRopaTipica.Services.IAuthService, InventarioRopaTipica.Services.AuthService>();
builder.Services.AddScoped<InventarioRopaTipica.Services.IUserService, InventarioRopaTipica.Services.UserService>();
builder.Services.AddScoped<InventarioRopaTipica.Services.IProductoService, InventarioRopaTipica.Services.ProductoService>();
builder.Services.AddScoped<InventarioRopaTipica.Services.IVentaService, InventarioRopaTipica.Services.VentaService>();
builder.Services.AddScoped<InventarioRopaTipica.Services.IClienteService, InventarioRopaTipica.Services.ClienteService>();
builder.Services.AddScoped<InventarioRopaTipica.Services.IProveedorService, InventarioRopaTipica.Services.ProveedorService>();

// SPA Static Files
builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "wwwroot";
});

var app = builder.Build();

// Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    // En desarrollo, lanzar el servidor de Vite automáticamente
    var clientAppPath = Path.Combine(Directory.GetCurrentDirectory(), "ClientApp");

    var viteProcess = new System.Diagnostics.Process
    {
        StartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "npm",
            Arguments = "run dev",
            WorkingDirectory = clientAppPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = false
        }
    };

    viteProcess.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
    viteProcess.ErrorDataReceived += (sender, args) => Console.WriteLine($"❌ Vite error: {args.Data}");

    viteProcess.Start();
    viteProcess.BeginOutputReadLine();
    viteProcess.BeginErrorReadLine();

    // Conecta el backend al servidor de desarrollo Vite
    app.UseSpa(spa =>
    {
        spa.Options.SourcePath = "ClientApp";
        spa.UseProxyToSpaDevelopmentServer("http://localhost:5173");
    });
}
else
{
    // En producción, sirve los archivos generados por Vite desde wwwroot
    app.UseSpa(spa => { spa.Options.SourcePath = "wwwroot"; });
}
// IMPORTANTE: Comentar esto en producción para evitar redirección HTTPS
// app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseSpaStaticFiles();

app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseTokenValidation();
app.UseAuthorization();

app.MapControllerRoute(
    name: "api",
    pattern: "api/{controller}/{action}/{id?}");

// Configuración SPA
app.UseSpa(spa =>
{
    spa.Options.SourcePath = "ClientApp";

    if (app.Environment.IsDevelopment())
    {
        // Ejecutar automáticamente Vite cuando se inicie el backend
        var clientAppPath = Path.Combine(Directory.GetCurrentDirectory(), "ClientApp");
        var viteProcess = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "npm",
                Arguments = "run dev",
                WorkingDirectory = clientAppPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = false
            }
        };

        viteProcess.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
        viteProcess.ErrorDataReceived += (sender, args) => Console.WriteLine($"❌ Vite error: {args.Data}");

        viteProcess.Start();
        viteProcess.BeginOutputReadLine();
        viteProcess.BeginErrorReadLine();

        // Proxy hacia el servidor de desarrollo de Vite
        spa.UseProxyToSpaDevelopmentServer("http://localhost:5173");
    }
    else
    {
        // En producción, sirve el contenido de wwwroot
        spa.Options.DefaultPageStaticFileOptions = new StaticFileOptions
        {
            OnPrepareResponse = ctx =>
            {
                ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store");
            }
        };
    }
});

app.Run();