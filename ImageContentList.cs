using System.Collections.Generic;

namespace DevTeam.ImageFormatter
{
    public class ImageContentList
    {
        public int? ImageId { get; set; }

        public ImageContentList()
        {
            Images = new List<ImageContent>();
        }
        public List<ImageContent> Images { get; set; }
    }
}
