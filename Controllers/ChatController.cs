using Microsoft.AspNetCore.Mvc;
using Lab01_Grupo1.Services;

namespace Lab01_Grupo1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly SemanticKernelService _semanticKernelService;
        private readonly ChatService _chatService;

        public ChatController(SemanticKernelService semanticKernelService, ChatService chatService)
        {
            _semanticKernelService = semanticKernelService;
            _chatService = chatService;
        }

        [HttpGet("{sessionId}")]
        public async Task<ActionResult> GetMessagesAsync(string sessionId)
        {
            var messages = await _chatService.GetMessagesAsync(sessionId);
            return Ok(messages);
        }

        [HttpPost]
        public async Task<ActionResult> GetChatResponseAsync([FromBody] ChatRequest chatRequest)
        {
            var sessionId = chatRequest.SessionId;
            var userMessage = chatRequest.UserMessage;

            await _chatService.AddMessageAsync(sessionId, userMessage, "User");
            var messages = await _chatService.GetMessagesAsync(sessionId);

            var response = await _semanticKernelService.ObtenerRespuestaConHistorialAsync(userMessage, messages);

            await _chatService.AddMessageAsync(sessionId, response, "Bot");

            return Ok(response);
        }
    }

    public record ChatRequest(string UserMessage, string SessionId);
}

// -----------------------------------------------------------------------------
// Controlador MVC para mostrar la vista del chatbot
// -----------------------------------------------------------------------------
namespace Lab01_Grupal.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
