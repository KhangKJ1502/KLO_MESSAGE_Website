using MessageWeb1.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MessageWeb1.Hubs
{
    public class ChatHub : Hub
    {
        private readonly KloMessageContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChatHub(KloMessageContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task SendMessage(string receiverUsername, string messageContent)
        {
            try {
                var senderUsername = _httpContextAccessor.HttpContext.Session.GetString("Username");

                if (string.IsNullOrEmpty(senderUsername))
                    throw new Exception("Người gửi không hợp lệ. Vui lòng đăng nhập lại.");

                var sender = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == senderUsername);

                var receiver = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == receiverUsername);

                if (sender == null)
                    throw new Exception("Người gửi không tồn tại trong hệ thống.");

                if (receiver == null)
                    throw new Exception("Người nhận không tồn tại trong hệ thống.");

                var conversation = await FindOrCreateConversation(sender.UserId, receiver.UserId);

                var message = new Message
                {
                    ConversationId = conversation.ConversationId,
                    SenderId = sender.UserId,
                    Content = messageContent,
                    SentAt = DateTime.Now,
                    IsDeleted = false
                };

                _context.Messages.Add(message);
                conversation.LastMessageId = message.MessageId;
                conversation.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                await Clients.User(receiver.Username).SendAsync("ReceiveMessage", sender.Username, messageContent, message.SentAt.ToString("o"));
                await Clients.Caller.SendAsync("ReceiveOwnMessage", messageContent, message.SentAt.ToString("o"));

                // Thông báo refresh cho cả 2 người
                await Clients.User(receiver.Username).SendAsync("RefreshMessages");
                await Clients.User(sender.Username).SendAsync("RefreshMessages");

            } catch (Exception ex) {
                Console.WriteLine($"[ChatHub] Lỗi khi gửi tin nhắn: {ex.Message}");
                throw;
            }
        }

        private async Task<Conversation> FindOrCreateConversation(int senderId, int receiverId)
        {
            var conversation = await _context.Conversations
                .Where(c => c.ConversationType == "private")
                .Where(c => c.ConversationMembers.Count == 2)
                .Where(c => c.ConversationMembers.Any(cm => cm.UserId == senderId))
                .Where(c => c.ConversationMembers.Any(cm => cm.UserId == receiverId))
                .FirstOrDefaultAsync();

            if (conversation != null)
                return conversation;

            conversation = new Conversation
            {
                ConversationType = "private",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            var members = new List<ConversationMember>
            {
                new ConversationMember { ConversationId = conversation.ConversationId, UserId = senderId, JoinedAt = DateTime.Now, IsAdmin = true },
                new ConversationMember { ConversationId = conversation.ConversationId, UserId = receiverId, JoinedAt = DateTime.Now, IsAdmin = false }
            };

            _context.ConversationMembers.AddRange(members);
            await _context.SaveChangesAsync();

            return conversation;
        }

        public async Task SendTypingIndicator(string receiverUsername)
        {
            var senderUsername = _httpContextAccessor.HttpContext.Session.GetString("Username");
            if (!string.IsNullOrEmpty(senderUsername)) {
                await Clients.User(receiverUsername).SendAsync("UserTyping", senderUsername);
            }
        }

        public override async Task OnConnectedAsync()
        {
            var username = _httpContextAccessor.HttpContext.Session.GetString("Username");
            if (!string.IsNullOrEmpty(username)) {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user != null) {
                    user.IsOnline = true;
                    user.LastSeen = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var username = _httpContextAccessor.HttpContext.Session.GetString("Username");
            if (!string.IsNullOrEmpty(username)) {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user != null) {
                    user.IsOnline = false;
                    user.LastSeen = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}