using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVTK
{
      public enum TypeMediaFile { Image, Video, Web }

    public class MultimediaFile
    {
        Uri Path { get; set; }//путь к файлу
        string Name { get; set; }//имя файла
        string Extension { get; set; }//расщирение файла
        TypeMediaFile typeMediaFile { get; set; }//Тип файла. Это позволяет разделить логику для видео, изображений и веба.
        int IdQueue { get; set; } // порядок в очереди плейлиста
        int Duration { get; set; }//длительность показа файла. Для видео нет смслы, но есть смысл для изображений и веб-страниц.
    }
}
