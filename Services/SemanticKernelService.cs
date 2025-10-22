using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.Configuration;
using Lab01_Grupo1.Data;
using System.Linq;

namespace Lab01_Grupo1.Services
{
    public class SemanticKernelService
    {
        private readonly Kernel _kernel;

        public SemanticKernelService(IConfiguration configuration)
        {
            string apiKey = configuration["OpenAI:ApiKey"];
            string model = configuration["OpenAI:Model"];

            _kernel = Kernel.CreateBuilder()
                .AddOpenAIChatCompletion(model, apiKey)
                .Build();
        }

        public async Task<string> ObtenerRespuestaAsync(string mensajeUsuario)
        {
            string prompt = $@"
Eres un asistente virtual médico de una página de citas médicas llamada *MediCitas+*.
Tu función es orientar al usuario sobre cómo registrarse, agendar una cita médica,
elegir el tipo de especialista adecuado y resolver dudas sobre el proceso (como horarios, pagos o tipos de consultas).

Responde siempre de forma amable, profesional, clara y breve.
Evita tecnicismos médicos complejos y enfócate en ayudar al usuario.

Usuario: {mensajeUsuario}
Chatbot:";

            var respuesta = await _kernel.InvokePromptAsync(prompt);
            return respuesta.ToString();
        }

        public async Task<string> ObtenerRespuestaConHistorialAsync(string mensajeUsuario, IEnumerable<ChatMessage> historial)
        {
            string contextoPrevio = string.Join("\n", historial.Select(m => $"{m.Sender}: {m.Message}"));

            string prompt = $@"
Eres un asistente virtual médico de *MediCitas+*.
Este es el historial de conversación:

{contextoPrevio}

El usuario ahora dice: {mensajeUsuario}

Responde con amabilidad y claridad sobre temas médicos o cómo sacar citas.";

            var respuesta = await _kernel.InvokePromptAsync(prompt);
            return respuesta.ToString();
        }
    }
}

