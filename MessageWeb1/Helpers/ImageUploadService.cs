using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace MessageWeb1.Helpers
{


    public class ImageUploadService
    {
        private readonly Cloudinary _cloudinary;

        public ImageUploadService()
        {
            Account account = new Account(
                "dsplgmtb6",
                "567827815694359",
                "8zcKwsntDSg5udMbo_LCMAAJ_iA");

            _cloudinary = new Cloudinary(account);
        }


        public string UploadImage(IFormFile file)
        {
            string uniqueFileName = Guid.NewGuid().ToString(); // tạo UUID

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(uniqueFileName, file.OpenReadStream()),
                PublicId = "avatars/" + uniqueFileName // tên ảnh trên Cloudinary
            };

            var uploadResult = _cloudinary.Upload(uploadParams);
            return uploadResult.SecureUrl.ToString(); // URL ảnh trên cloud
        }
    }

}
