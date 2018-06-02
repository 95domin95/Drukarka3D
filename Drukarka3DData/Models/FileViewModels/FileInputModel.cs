using Microsoft.AspNetCore.Http;

namespace Drukarka3DData.Models
{
    public class FileInputModel
    {
        public IFormFile FileToUpload { get; set; }
    }
}
