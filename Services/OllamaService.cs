using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Lab01_Grupo1.Models;
using System.Threading;

namespace Lab01_Grupo1.Services
{
    public class OllamaService
    {
        private readonly OllamaSettings _settings;
        private readonly HttpClient _http;
        private readonly CacheService _cacheService;

        public OllamaService(IOptions<OllamaSettings> settings, CacheService cacheService)
        {
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _cacheService = cacheService;

            _http = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(2) // ✅ Aumentar timeout global
            };
        }

        // ======================================================
        //   MÉTODO PRINCIPAL MEJORADO
        // ======================================================
        public async Task<string> AskOllamaAsync(string userMessage, string pageContext)
        {
            // 🔥 PRIMERO: Buscar respuesta rápida predefinida
            var quickResponse = GetQuickResponse(userMessage, pageContext);
            if (quickResponse != null)
                return quickResponse;

            // 🔥 SEGUNDO: Usar caché
            var cacheKey = $"ollama_{userMessage.ToLower().Trim().GetHashCode()}";

            return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                var url = $"{_settings.BaseUrl}/api/generate";

                // 🔥 PROMPT MEJORADO para asistente médico específico
                var systemPrompt = $@"
Eres MedAssist, el asistente médico oficial de E-CORP. Estás en la página: {pageContext}

INSTRUCCIONES ESPECÍFICAS:
- Responde como experto médico amigable
- Explica procesos claramente: agendar citas, formularios, horarios, precios
- Sé práctico y da ejemplos cuando pidan llenar formularios
- Si no sabes algo, sugiere contactar al consultorio
- Responde en español, máximo 4-5 oraciones

Pregunta del usuario: {userMessage}

Respuesta útil:";

                var requestBody = new
                {
                    model = _settings.Model,
                    prompt = systemPrompt,
                    stream = false,
                    options = new { 
                        temperature = 0.7,
                        num_predict = 150  // ✅ Permitir respuestas más completas
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // ✅ TIMEOUT MÁS LARGO para primera ejecución
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
                
                try
                {
                    var response = await _http.PostAsync(url, content, cts.Token);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        // ✅ Respuesta de respaldo si falla Ollama
                        return GetFallbackResponse(userMessage, pageContext);
                    }

                    var responseText = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(responseText);
                    var fullResponse = doc.RootElement.GetProperty("response").GetString()
                                       ?? GetFallbackResponse(userMessage, pageContext);

                    return fullResponse;

                }
                catch (TaskCanceledException)
                {
                    // ✅ Respuesta útil en lugar del mensaje genérico
                    return GetTimeoutResponse(userMessage, pageContext);
                }
                catch (Exception ex)
                {
                    return GetFallbackResponse(userMessage, pageContext);
                }
            }, TimeSpan.FromMinutes(120)); // ✅ Cache por 2 horas
        }

        // ======================================================
        //   RESPUESTAS RÁPIDAS ESPECÍFICAS PARA E-CORP
        // ======================================================
        private string GetQuickResponse(string userMessage, string pageContext)
        {
            var lowerMessage = userMessage.ToLower();

            // 🔥 SALUDOS
            if (lowerMessage.Contains("hola") || lowerMessage.Contains("buenos días") || lowerMessage.Contains("buenas tardes") || lowerMessage.Contains("hi"))
                return "¡Hola! Soy MedAssist, tu asistente médico en E-CORP. Puedo ayudarte con:\n• Agendar citas médicas\n• Explicar formularios de salud\n• Información de horarios y precios\n• Responder preguntas médicas generales\n\n¿En qué puedo asistirte hoy?";

            // 🔥 FORMULARIOS
            if (lowerMessage.Contains("formulario") || lowerMessage.Contains("llenar") || lowerMessage.Contains("casilla") || lowerMessage.Contains("ejemplo"))
                return "**Formulario de Salud Inicial**:\n\nEjemplo para llenar:\n• **Nombre completo**: Juan Pérez López\n• **Edad**: 35 años\n• **Síntomas principales**: Dolor de cabeza persistente\n• **Medicamentos actuales**: Paracetamol 500mg\n• **Alergias**: Ninguna\n• **Antecedentes familiares**: Diabetes\n\n💡 **Consejo**: Sé específico con tus síntomas y menciona todos los medicamentos que tomas.";

            // 🔥 CITAS
            if (lowerMessage.Contains("cita") || lowerMessage.Contains("agendar") || lowerMessage.Contains("consultorio") || lowerMessage.Contains("reservar"))
                return "**Para agendar citas en E-CORP**:\n\n1. Ve a 'Agendar Cita' en tu panel\n2. Selecciona especialidad: Medicina General, Cardiología, etc.\n3. Elige fecha y hora disponible\n4. Confirma tus datos\n\n**Horario**: Lunes a Viernes 8:00 AM - 6:00 PM\n**Costo consulta**: $50 (Medicina General)\n**Urgencias**: Disponibles sin cita previa";

            // 🔥 PRECIOS
            if (lowerMessage.Contains("precio") || lowerMessage.Contains("costo") || lowerMessage.Contains("cuánto") || lowerMessage.Contains("valor"))
                return "**Tarifas E-CORP**:\n\n• Consulta Medicina General: $50\n• Consulta Especialista: $80\n• Chequeo anual completo: $120\n• Urgencias: $75\n• Estudios de laboratorio: Desde $30\n\n💳 Aceptamos todos los seguros médicos principales.";

            // 🔥 HORARIOS
            if (lowerMessage.Contains("horario") || lowerMessage.Contains("atención") || lowerMessage.Contains("abierto") || lowerMessage.Contains("cierra"))
                return "**Horarios de Atención E-CORP**:\n\n🏥 **Lunes a Viernes**: 8:00 AM - 6:00 PM\n🏥 **Sábados**: 9:00 AM - 1:00 PM\n🏥 **Domingos**: Cerrado\n🏥 **Urgencias**: 24/7\n\n📞 **Contacto**: (555) 123-4567";

            // 🔥 CONTACTO
            if (lowerMessage.Contains("contacto") || lowerMessage.Contains("teléfono") || lowerMessage.Contains("llamar") || lowerMessage.Contains("email"))
                return "**Contacto E-CORP**:\n\n📞 Teléfono: (555) 123-4567\n📧 Email: info@ecorp-med.com\n🏢 Dirección: Av. Médica 123, Ciudad\n🌐 Website: www.ecorp-med.com\n\n**Horario de contacto**: Lunes a Viernes 7:00 AM - 7:00 PM";

            // 🔥 GRACIAS
            if (lowerMessage.Contains("gracias") || lowerMessage.Contains("thank you") || lowerMessage.Contains("agradecido"))
                return "¡De nada! 😊 Estoy aquí para ayudarte en todo lo que necesites con tus citas médicas, formularios de salud o cualquier pregunta. ¡Que tengas un excelente día en E-CORP!";

            return null;
        }

        // ======================================================
        //   RESPUESTAS DE FALLBACK MEJORADAS
        // ======================================================
        private string GetTimeoutResponse(string userMessage, string pageContext)
        {
            return "⏳ Estoy procesando tu consulta médica. Mientras tanto, te puedo informar:\n\n" + 
                   GetQuickResponse(userMessage, pageContext) ?? 
                   "Puedes contactarnos directamente al (555) 123-4567 para asistencia inmediata.";
        }

        private string GetFallbackResponse(string userMessage, string pageContext)
        {
            var lowerMessage = userMessage.ToLower();
            
            if (lowerMessage.Contains("formulario") || lowerMessage.Contains("llenar"))
                return "**Ejemplo para Formulario de Salud**:\n• Nombre: María González\n• Edad: 28\n• Síntomas: Fiebre y tos por 3 días\n• Medicamentos: Ibuprofeno\n• Alergias: Penicilina\n\n💡 Llena todos los campos con información veraz para mejor diagnóstico.";

            if (lowerMessage.Contains("cita") || lowerMessage.Contains("agendar"))
                return "Para agendar cita: Ve a 'Agendar Cita' → Elige especialidad → Selecciona fecha/hora → Confirma. Horario: L-V 8AM-6PM, Sábados 9AM-1PM.";

            return "¡Hola! Soy MedAssist de E-CORP. Puedo ayudarte con:\n• Agendar citas médicas\n• Formularios de salud\n• Información de horarios y precios\n• Preguntas médicas generales\n\n¿En qué necesitas ayuda específicamente?";
        }
    }
}
