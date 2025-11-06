using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lab01_Grupo1.Models;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using Lab01_Grupo1.Services;

namespace Lab01_Grupo1.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly ILogger<AdminController> _logger;
        private readonly TimeSpan _cacheTtl;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        };

        private readonly MLModelService _mlService;
        private readonly IWebHostEnvironment _env;

        public AdminController(
            ApplicationDbContext context,
            IDistributedCache cache,
            ILogger<AdminController> logger,
            IConfiguration configuration,
            MLModelService mlService,
            IWebHostEnvironment env
        )
        {
            _context = context;
            _cache = cache;
            _logger = logger;
            _mlService = mlService;
            _env = env;

            var ttlMinutes = configuration.GetValue<int>("Redis:CacheTtlMinutes", 2);
            _cacheTtl = TimeSpan.FromMinutes(Math.Min(ttlMinutes, 2));
        }

        // Página principal
        public IActionResult Index() => View();

        public IActionResult Powerbi() => View();

        // ✅ Lista de Citas
        [HttpGet]
        public async Task<IActionResult> ListaCitas()
        {
            try
            {
                var citas = await _context.Citas
                    .Include(c => c.Paciente)
                    .Include(c => c.Medico)
                        .ThenInclude(m => m.Usuario)
                    .ToListAsync();

                return View(citas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar las citas");
                return View(new List<Cita>());
            }
        }

        // ✅ Lista de Usuarios
        [HttpGet]
        [Route("Admin/ListaUsuarios")]
        public async Task<IActionResult> ListaUsuarios(string? nombre = null)
        {
            var normalized = string.IsNullOrWhiteSpace(nombre) ? null : nombre.Trim();
            var cacheKey = normalized == null
                ? "admin:usuarios:lista"
                : $"admin:usuarios:lista:search:{normalized.ToLowerInvariant()}";

            try
            {
                var cached = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cached))
                {
                    _logger.LogInformation("Cache HIT ListaUsuarios (search: {Search})", normalized);
                    var cachedList = JsonSerializer.Deserialize<List<Usuario>>(cached, _jsonOptions)
                                     ?? new List<Usuario>();
                    ViewData["Search"] = normalized;
                    return View(cachedList);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error leyendo cache en ListaUsuarios");
            }

            var query = _context.Set<Usuario>().Where(u => u.Rol == "usuario");

            if (!string.IsNullOrWhiteSpace(normalized))
            {
                var lower = normalized.ToLowerInvariant();
                query = query.Where(u =>
                    (u.Nombre != null && EF.Functions.Like(u.Nombre.ToLower(), $"%{lower}%")) ||
                    (u.UsuarioNombre != null && EF.Functions.Like(u.UsuarioNombre.ToLower(), $"%{lower}%")));
            }

            var usuarios = await query
                .Select(u => new Usuario
                {
                    IdUsuario = u.IdUsuario,
                    UsuarioNombre = u.UsuarioNombre,
                    Nombre = u.Nombre,
                    Correo = u.Correo,
                    Rol = u.Rol
                })
                .AsNoTracking()
                .ToListAsync();

            try
            {
                var serialized = JsonSerializer.Serialize(usuarios, _jsonOptions);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _cacheTtl
                };
                await _cache.SetStringAsync(cacheKey, serialized, options);
                _logger.LogInformation("ListaUsuarios guardada en cache (search: {Search})", normalized);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error guardando cache en ListaUsuarios");
            }

            ViewData["Search"] = normalized;
            return View(usuarios);
        }

        // ✅ Registro de Contactos
        [HttpGet]
        public async Task<IActionResult> RegistroContacto()
        {
            try
            {
                var contactos = await _context.Contactos
                    .OrderByDescending(c => c.FechaEnviado)
                    .ToListAsync();

                return View(contactos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar RegistroContacto");
                return View(new List<Contacto>());
            }
        }

        // ✅ Clasificación de Contactos + CSV + ML.NET
        [HttpGet]
        public async Task<IActionResult> ClasificacionContactos()
        {
            try
            {
                var contactos = await _context.Contactos
                    .OrderByDescending(c => c.FechaEnviado)
                    .ToListAsync();

                // 🔹 Ruta al archivo CSV dentro del proyecto
                string csvPath = Path.Combine(
                    _env.ContentRootPath,
                    "ML", "Data", "config", "Training data", "Prueba",
                    "comentarios_consultas_medicas.csv"
                );

                _logger.LogInformation("Ruta esperada del CSV: {Path}", csvPath);

                var listaCsv = new List<ComentarioAnalizado>();

                if (System.IO.File.Exists(csvPath))
                {
                    _logger.LogInformation("✅ Archivo CSV encontrado. Procesando datos...");

                    foreach (var line in System.IO.File.ReadLines(csvPath).Skip(1))
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        // Divide por comas, pero ignora líneas con menos columnas
                        var parts = line.Split(',');
                        if (parts.Length < 1) continue;

                        var comentario = parts.Last().Trim();

                        if (!string.IsNullOrWhiteSpace(comentario))
                        {
                            var (prediccion, confianza) = _mlService.AnalizarSentimiento(comentario);
                            listaCsv.Add(new ComentarioAnalizado
                            {
                                Comentario = comentario,
                                EtiquetaPredicha = prediccion,
                                Puntuacion = confianza
                            });
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("⚠️ No se encontró el archivo CSV en {Path}", csvPath);
                }

                // 🔹 También analiza los mensajes de Contactos (BD)
                var listaContactos = new List<ComentarioAnalizado>();
                foreach (var c in contactos)
                {
                    var (prediccion, confianza) = _mlService.AnalizarSentimiento(c.Mensaje);
                    listaContactos.Add(new ComentarioAnalizado
                    {
                        Comentario = c.Mensaje,
                        EtiquetaPredicha = prediccion,
                        Puntuacion = confianza
                    });
                }

                // 🔸 Combinar CSV + Contactos
                var listaFinal = listaCsv.Concat(listaContactos).ToList();

                // 🔸 Ordenar resultados por tipo de sentimiento
                listaFinal = listaFinal
                    .OrderByDescending(c => c.EtiquetaPredicha == "Positivo")
                    .ThenByDescending(c => c.EtiquetaPredicha == "Neutro")
                    .ThenByDescending(c => c.Puntuacion)
                    .ToList();

                return View(listaFinal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ClasificacionContactos");
                return View(new List<ComentarioAnalizado>());
            }
        }
    }
}



