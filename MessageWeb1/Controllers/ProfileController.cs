using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using MessageWeb1.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MessageWeb1.Models;

namespace MessageWeb1.Controllers
{
    public class ProfileController : Controller
    {
        private readonly KloMessageContext _context;

        public ProfileController(KloMessageContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("api/upload-avatar")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            var currentUser = HttpContext.Session.GetInt32("UserId");

            if (currentUser == null) {
                return Unauthorized("Chưa đăng nhập hoặc không tìm thấy phiên người dùng.");
            }

            if (file != null && file.Length > 0) {
                var imageService = new ImageUploadService();
                string imageUrl = imageService.UploadImage(file); // Upload image to cloud and get the URL

                var user = _context.Users.FirstOrDefault(u => u.UserId == currentUser);
                if (user != null) {
                    Console.WriteLine(user.AvatarUrl);
                    user.AvatarUrl = imageUrl; // Save URL in the database
                    _context.SaveChanges();
                    ViewBag.ImageUrl = imageUrl;
                    return Ok(new { imageUrl }); // Return URL as JSON response
                }
                else {
                    return NotFound("Không tìm thấy người dùng.");
                }
            }

            return BadRequest("Không có file hợp lệ");
        }
    }


}
