
using DBS.Models;
using DBS.Repositories;
using DBS.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// JWT: prioriza a variável configurada no Render.
var jwtSecret = builder.Configuration["JWT_SECRET"]
    ?? builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtSecret))
{
    jwtSecret = Convert.ToBase64String(
        RandomNumberGenerator.GetBytes(48));

    builder.Configuration["JWT_SECRET"] = jwtSecret;
}

var jwtKey = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(jwtSecret));

// MVC e API
builder.Services.AddControllersWithViews();

// Autenticação JWT para o aplicativo mobile e API
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = jwtKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

// CORS para o aplicativo mobile
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirMobile", policy =>
        policy.SetIsOriginAllowed(origin =>
        {
            if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                return false;

            return uri.Host.Equals(
                       "localhost",
                       StringComparison.OrdinalIgnoreCase)
                   || uri.Host.EndsWith(
                       ".app.github.dev",
                       StringComparison.OrdinalIgnoreCase);
        })
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// Sessão
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Repositórios existentes
builder.Services.AddScoped<ClienteRepository>();
builder.Services.AddScoped<ProdutoRepository>();
builder.Services.AddScoped<PedidoRepository>();
builder.Services.AddScoped<DashboardRepository>();

// Serviços e repositórios do aplicativo mobile
builder.Services.AddScoped<VendedorAppRepository>();
builder.Services.AddScoped<PedidoAppRepository>();
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddSingleton<IPasswordHasher<string>, PasswordHasher<string>>();

// Repositório e autenticação por perfil
builder.Services.AddScoped<VendedorRepository>();
builder.Services.AddScoped<
    IPasswordHasher<Vendedor>,
    PasswordHasher<Vendedor>>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("PermitirMobile");

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.MapControllers();

app.Run();
