using Lab01_Grupo1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http; 
using StackExchange.Redis;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Lab01_Grupo1.Configuration;
using Lab01_Grupo1.Services;
using Braintree;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// 🔹 Necesario para inyectar IHttpContextAccessor en Razor (@inject)
builder.Services.AddHttpContextAccessor();

// 🔹 Cache para que funcionen las sesiones
builder.Services.AddDistributedMemoryCache();

// =========================================================
// 💳 CONFIGURACIÓN DEL CLIENTE BRAINTREE
// =========================================================
// Registra IBraintreeGateway como Singleton (se crea una vez)
builder.Services.AddSingleton<IBraintreeGateway>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();

    // CORRECCIÓN CS0104: Usamos Braintree.Environment para evitar la ambigüedad con System.Environment
    Braintree.Environment environment = config["Braintree:Environment"]?.ToLower() == "production"
    ? Braintree.Environment.PRODUCTION
    : Braintree.Environment.SANDBOX;

    return new BraintreeGateway
    {
        // CORRECCIÓN CS0104: Usamos Braintree.Environment.SANDBOX/PRODUCTION
        Environment = environment,
        MerchantId = config["Braintree:MerchantId"],
        PublicKey = config["Braintree:PublicKey"],
        PrivateKey = config["Braintree:PrivateKey"],
        // 🚨 CAMBIO FINAL: Se elimina la propiedad 'PayPalMerchantAccountId' que causó el error CS0117.
        // La habilitación de PayPal debe manejarse en el lado del cliente (JavaScript) usando el token generado.
    };
});

// CORRECCIÓN CS0104: Usamos el namespace completo para nuestro servicio
builder.Services.AddScoped<Lab01_Grupo1.Services.BraintreeService>(); 
// =========================================================

// 🧠 Agregamos el servicio del Chatbot con OpenAI (SemanticKernelService)
builder.Services.AddSingleton<SemanticKernelService>();
builder.Services.AddScoped<ChatService>();

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