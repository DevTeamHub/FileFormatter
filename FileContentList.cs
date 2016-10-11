using System.Collections.Generic;

namespace DevTeam.FileFormatter
{
    public class FileContentList
    {
        public int? ImageId { get; set; }

        public FileContentList()
        {
            Images = new List<FileContent>();
        }
        public List<FileContent> Images { get; set; }
    }
}
