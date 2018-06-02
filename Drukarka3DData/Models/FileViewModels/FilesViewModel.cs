using System.Collections.Generic;

namespace Drukarka3DData.Models
{
    public class FileDetails
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }

    public class FilesViewModel
    {
        public List<FileDetails> Files { get; set; } 
            = new List<FileDetails>();
    }
}
