using DBS.Repositories;
using DBS.Services;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// JWT: usa o segredo do ambiente; se ausente, gera uma chave temporária
// apenas em memória para não impedir o sistema web de iniciar.
var jwtSecret = builder.Configuration["JWT_SECRET"];
if (string.IsNullOrWhiteSpace(jwtSecret))
{
    jwtSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
    builder.Configuration["JWT_SECRET"] = jwtSecret;
}

var jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "DBS.ERP",
            ValidateAudience = true,
            ValidAudience = "DBS.Mobile",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = jwtKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

// MVC
builder.Services.AddControllersWithViews();

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirMobile", policy =>
        policy.SetIsOriginAllowed(origin =>
        {
            if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                return false;

            return uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                || uri.Host.EndsWith(".app.github.dev", StringComparison.OrdinalIgnoreCase);
        })
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// Sessão (para o login funcionar)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Injeção de dependência dos repositórios
builder.Services.AddScoped<ClienteRepository>();
builder.Services.AddScoped<ProdutoRepository>();
builder.Services.AddScoped<PedidoRepository>();
builder.Services.AddScoped<DashboardRepository>();
builder.Services.AddScoped<VendedorAppRepository>();
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddSingleton<IPasswordHasher<string>, PasswordHasher<string>>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("PermitirMobile"); // precisa vir depois de UseRouting e antes de UseAuthorization

app.UseSession(); // mantém o login web atual
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.MapControllers(); // necessário para os Controllers com [ApiController] (rotas api/...) funcionarem

app.Run();