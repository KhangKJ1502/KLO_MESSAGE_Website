using MessageWeb1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace MessageWeb1.Controllers
{
    public class LoginController : Controller
    {
        private readonly KloMessageContext _context;

        public LoginController(KloMessageContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index(string returnUrl = "/Chat/Index")
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string username, string password, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (string.IsNullOrWhiteSpace(username)) {
                ViewBag.Error = "Vui lòng nhập tên đăng nhập";
                return View();
            }

            if (string.IsNullOrWhiteSpace(password)) {
                ViewBag.Error = "Vui lòng nhập mật khẩu";
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null) {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng";
                return View();
            }

            // Hash mật khẩu để so sánh (nếu bạn đang hash)
            // string passwordHash = HashPassword(password);
            string passwordHash = password;

            if (user.PasswordHash != passwordHash) {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng";
                return View();
            }

            // Lưu thông tin người dùng vào session
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetInt32("UserId", user.UserId);

            // ✅ Đăng nhập bằng Cookie Authentication
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // ✅ Cập nhật trạng thái online
            user.IsOnline = true;
            user.LastSeen = DateTime.Now;
            await _context.SaveChangesAsync();

            // Điều hướng đến trang mặc định nếu returnUrl không hợp lệ
            if (!Url.IsLocalUrl(returnUrl)) {
                returnUrl = "/Chat/Index";
            }

            return Redirect(returnUrl);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            // Cập nhật trạng thái offline
            var username = HttpContext.Session.GetString("Username");
            if (!string.IsNullOrEmpty(username)) {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user != null) {
                    user.IsOnline = false;
                    user.LastSeen = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
            }

            // Xóa session & đăng xuất
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Login");
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create()) {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hash.Length; i++) {
                    builder.Append(hash[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}
