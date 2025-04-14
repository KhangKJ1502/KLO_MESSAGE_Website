using MessageWeb1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MessageWeb1.Controllers
{
    public class ChatController : Controller
    {
        private readonly KloMessageContext _context;

        public ChatController(KloMessageContext context)
        {
            _context = context;
        }
        // ChatController.cs

        [HttpGet]
        public async Task<IActionResult> Index(string toUser)
        {
            // Existing authentication code...
            var currentUser = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(currentUser)) {
                var returnUrl = string.IsNullOrEmpty(toUser)
                    ? "/Chat/Index"
                    : $"/Chat/Index?toUser={toUser}";

                return RedirectToAction("Index", "Login", new { returnUrl });
            }

            // Existing user lookup code...
            if (string.IsNullOrEmpty(toUser)) {
                return RedirectToAction("SelectUser");
            }

            // Find user information
            var currentUserEntity = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == currentUser);

            var toUserEntity = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == toUser);

            var listFriend = await _context.UserContacts
                .Where(uc => uc.UserId == currentUserEntity.UserId)
                .Include(uc => uc.Contact)
                .ToListAsync();

            if (currentUserEntity == null || toUserEntity == null) {
                return NotFound("Không tìm thấy người dùng");
            }

            // Find conversation between the two users
            var conversation = await _context.Conversations
                .Where(c => c.ConversationType == "private" &&
                            c.ConversationMembers.Count == 2 &&
                            c.ConversationMembers.Any(cm => cm.UserId == currentUserEntity.UserId) &&
                            c.ConversationMembers.Any(cm => cm.UserId == toUserEntity.UserId))
                .FirstOrDefaultAsync();

            // Get messages to display
            var messages = new List<Message>();

            if (conversation != null) {
                messages = await _context.Messages
                    .Where(m => m.ConversationId == conversation.ConversationId && !m.IsDeleted)
                    .OrderBy(m => m.SentAt)
                    .Include(m => m.Sender)
                    .ToListAsync();
            }

            // Pass data via ViewBag
            ViewBag.CurrentUser = currentUser;
            ViewBag.CurrentUserAvatar = currentUserEntity.AvatarUrl ?? "/Image/b55138a4-1adb-4a77-bc4d-7ac35be450e6.png\"";
            ViewBag.ToUser = toUser;
            ViewBag.ToUserAvatar = toUserEntity.AvatarUrl ?? "/Image/b55138a4-1adb-4a77-bc4d-7ac35be450e6.png\"";
            ViewBag.ToUserOnline = toUserEntity.IsOnline;
            ViewBag.ToUserLastSeen = toUserEntity.LastSeen;
            ViewBag.Conversation = messages;
            ViewBag.ListFriend = listFriend;

            return View();
        }

        public async Task<IActionResult> SelectUser()
        {
            var currentUser = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(currentUser)) {
                return RedirectToAction("Index", "Login");
            }

            // Lấy danh sách người dùng khác ngoại trừ người dùng hiện tại
            var otherUsers = await _context.Users
                .Where(u => u.Username != currentUser)
                .ToListAsync();

            ViewBag.CurrentUser = currentUser;

            return View(otherUsers);
        }
    }
}