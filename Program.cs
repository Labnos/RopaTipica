using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using InventarioRopaTipica.Data;
using InventarioRopaTipica.Middleware;

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
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
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
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
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
        // En desarrollo, proxy a Vite
        spa.UseProxyToSpaDevelopmentServer("http://localhost:5173");
    }
});

app.Run();