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
            public IActionResult Login()
            {
                return RedirectToAction("Index", "Login"); // chuyển hướng về LoginController
            }


        public async Task<IActionResult> Index(string toUser)
        {
            // Kiểm tra đăng nhập
            var currentUser = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(currentUser)) {
                var returnUrl = string.IsNullOrEmpty(toUser)
                    ? "/Chat/Index"
                    : $"/Chat/Index?toUser={toUser}";

                return RedirectToAction("Index", "Login", new { returnUrl });
            }

            // Nếu không có người nhận, chuyển đến trang chọn người nhận
            if (string.IsNullOrEmpty(toUser)) {
                return RedirectToAction("SelectUser");
            }

            // Tìm thông tin người dùng
            var currentUserEntity = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == currentUser);

            var toUserEntity = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == toUser);

            if (currentUserEntity == null || toUserEntity == null) {
                return NotFound("Không tìm thấy người dùng");
            }

            // Tìm conversation giữa hai người dùng
            var conversation = await _context.Conversations
      .Where(c => c.ConversationType == "private" &&
                  c.ConversationMembers.Count == 2 &&
                  c.ConversationMembers.Any(cm => cm.UserId == currentUserEntity.UserId) &&
                  c.ConversationMembers.Any(cm => cm.UserId == toUserEntity.UserId))
      .FirstOrDefaultAsync();

            // Lấy tin nhắn để hiển thị
            var messages = new List<Message>();

            if (conversation != null) {
                messages = await _context.Messages
                    .Where(m => m.ConversationId == conversation.ConversationId && !m.IsDeleted)
                    .OrderBy(m => m.SentAt)
                    .Include(m => m.Sender)
                    .ToListAsync();
            }

            // Truyền dữ liệu qua ViewBag
            ViewBag.CurrentUser = currentUser;
            ViewBag.ToUser = toUser;
            ViewBag.Conversation = messages;

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