using Microsoft.EntityFrameworkCore;
using Lab01_Grupo1.Data;
using Lab01_Grupo1.Models;

namespace Lab01_Grupo1.Services
{
    public class ChatService
    {
        private readonly ApplicationDbContext _dbContext;

        public ChatService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddMessageAsync(string sessionId, string message, string sender)
        {
            var chatMessage = new ChatMessage
            {
                SessionId = sessionId,
                Message = message,
                Sender = sender,
                Timestamp = DateTime.UtcNow
            };

            _dbContext.ChatMessages.Add(chatMessage);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<ChatMessage>> GetMessagesAsync(string sessionId)
        {
            return await _dbContext.ChatMessages
                .Where(m => m.SessionId == sessionId)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }
    }
}
