using Lab01_Grupo1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http; 
using StackExchange.Redis;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Lab01_Grupo1.Configuration;
using Lab01_Grupo1.Services;
using Braintree;
// 🔹 Agregar el using para NoticiasMedicas
using NoticiasMedicas.Services;
// 🔹 Agregar el using del modelo ML.NET
using Lab01_Grupo1.Models;
using static Lab01_Grupo1.Models.MLModel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
// 🔹 Registro del servicio OllamaService
builder.Services.AddSingleton<OllamaService>();
builder.Services.AddSingleton<CacheService>();

builder.Services.AddHttpClient();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// 🔹 Necesario para inyectar IHttpContextAccessor en Razor (@inject)
builder.Services.AddHttpContextAccessor();

// 🔹 Cache para que funcionen las sesiones
builder.Services.AddDistributedMemoryCache();

// 🔹 REGISTRAR EL SERVICIO DE NOTICIAS MÉDICAS
builder.Services.AddHttpClient<MedicalNewsService>();

// =========================================================
// 💳 CONFIGURACIÓN DEL CLIENTE BRAINTREE
// =========================================================
builder.Services.AddSingleton<IBraintreeGateway>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();

    Braintree.Environment environment = config["Braintree:Environment"]?.ToLower() == "production"
    ? Braintree.Environment.PRODUCTION
    : Braintree.Environment.SANDBOX;

    return new BraintreeGateway
    {
        Environment = environment,
        MerchantId = config["Braintree:MerchantId"],
        PublicKey = config["Braintree:PublicKey"],
        PrivateKey = config["Braintree:PrivateKey"],
    };
});

// CORRECCIÓN CS0104
builder.Services.AddScoped<Lab01_Grupo1.Services.BraintreeService>(); 
// =========================================================

// 🧠 Servicio Chatbot OpenAI
builder.Services.AddSingleton<SemanticKernelService>();
builder.Services.AddScoped<ChatService>();

builder.Services.AddHttpClient<OpenFDAService>(); //API OPENFDA

// 🔹 Servicio del modelo ML.NET
builder.Services.AddSingleton<MLModelService>();

builder.Services.AddHttpClient<OpenFDAService>(client =>
{
    client.BaseAddress = new Uri("https://api.fda.gov/");
    client.DefaultRequestHeaders.Add("User-Agent", "Lab01-Grupo1/1.0");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// 🔹 Sesión
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 👉👉👉 **AQUÍ AGREGO TU CÓDIGO DE OLLAMA**
// ============================================================
builder.Services.Configure<OllamaSettings>(
    builder.Configuration.GetSection("Ollama"));

builder.Services.AddSingleton<OllamaService>();
// ============================================================

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Redis
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
    // fallback
}

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
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
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
