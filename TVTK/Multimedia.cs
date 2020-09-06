using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVTK
{
    public enum Type
    {
        Video,
        Document,
        Photo
    }

    public class MultimediaFile
    {
        // public Url url { get; set; }
        public int id { get; set; }
        public Type type { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
    }
}
