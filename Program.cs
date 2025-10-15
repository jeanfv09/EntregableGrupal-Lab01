using Lab01_Grupo1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http; // 👈 necesario para IHttpContextAccessor
using StackExchange.Redis;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Lab01_Grupo1.Configuration;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// 🔹 Necesario para inyectar IHttpContextAccessor en Razor (@inject)
builder.Services.AddHttpContextAccessor();

// 🔹 Cache para que funcionen las sesiones
builder.Services.AddDistributedMemoryCache();

// Para la implementacion del API paypal
builder.Services.Configure<PayPalOptions>(
builder.Configuration.GetSection(PayPalOptions.PayPal));

// 🔹 Configuración de la sesión
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Sesión dura 30 min
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
// 1) Configuración servicios (DbContext, Identity si aplica, Cache, Session, Redis)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddHttpContextAccessor();

// Session + distributed cache (default to memory; try replace with Redis below)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Redis - IMPORTANT: register BEFORE builder.Build()
var redisHost = builder.Configuration["Redis:Host"];
var redisPort = builder.Configuration.GetValue<int>("Redis:Port", 6379);
var redisUser = builder.Configuration["Redis:User"];
var redisPassword = builder.Configuration["Redis:Password"];

try
{
    var configurationOptions = new ConfigurationOptions
    {
        AbortOnConnectFail = false,
        ConnectTimeout = 5000
    };
    configurationOptions.EndPoints.Add(redisHost, redisPort);
    configurationOptions.User = redisUser;
    configurationOptions.Password = redisPassword;

    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.ConfigurationOptions = configurationOptions;
    });

    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
        ConnectionMultiplexer.Connect(configurationOptions));
}
catch
{
    // keep memory cache already registered as fallback
}
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // 🔹 Página de errores detallados en desarrollo
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 🔹 Habilitar sesiones ANTES de Authorization
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
