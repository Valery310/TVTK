using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Windows.Forms;

namespace TVTK
{

    public enum TypeMediaFile { Image, Video, Web }

    public class MultimediaFile
    {
        Uri Path { get; set; }//путь к файлу
        string Name { get; set; }//имя файла
        string Extension { get; set; }//расщирение файла
        TypeMediaFile typeMediaFile { get; set; }//Тип файла. Это позволяет разделить логику для видео, изображений и веба.
        int Duration { get; set; }//длительность показа файла. Для видео нет смслы, но есть смысл для изображений и веб-страниц.
    }

    public class PlayList
    {
        public static List<uint> historyPlaying { get; private set; }//История проигрывания, чтобы можно было веронуться к предыдущему файлу.
        public uint CurrentIndex { get; private set; } // Индекс текущего проигрываемого файла
        public string NamePlalist { get; private set; }// Имя плейлиста
        public Dictionary<uint, MultimediaFile> MediaFiles { get; private set; }//порядок воспроизведения зависит от ключа.
        public int DurationShow { get; set; }//длительность показа контента плейлиста. Например: Показывать в течении 10 минут.
        public int FrequencyShow { get; set; }//показывать раз в столько то минут. Например: каждые 20 минут.
        public Dictionary<DayOfWeek, List<Time>> TimeOfPlaying { get; private set; }// в разные дни недели может быть разное время проигрывания плейлиста
        public TypeContent typeContent { get; private set; } //может пригодится во время обработок плелйиста. Пока не придумал =)
        public TypePlaying typePlaying { get; private set; }//Порядок проигрывания файлов в плелисте.

        public PlayList(string Name)
        {
            NamePlalist = Name;
        }

        public void NewPlaylist(List<Uri> uris) { }
        public void NewPlaylist(List<FileInfo> fileInfos) { }
        public void NewPlaylist(List<string> filesPath) { }
        public void NewPlaylist(DirectoryInfo directoryInfo, SearchOption searchOption) { }
        public void EditPlaylist() { }//Редактирование параметров плейлиста, кроме списка файлов и их порядка проигрывания.
        public void DeletePlaylist() { }//Удаление всего объекта. Чистка переменных.

        public void EditPositionFile(MultimediaFile multimediaFile, uint position) { }
        public void DeletePositionFile(MultimediaFile multimediaFile, uint position) { }
        public void AddPositionFile(MultimediaFile multimediaFile) { }

        public void SetDurationShow(uint minute = 0) { }
        public void SetFrequencyShow(uint minute = 0) { }

        public void SetTimeOfPlaying(DayOfWeek[] dayOfWeeks, List<Time> times) { }

        public void SetTypeContent(TypeContent typeContent = TypeContent.Default) { }

        public MultimediaFile NextMediaFile() { return MediaFiles[NextIndex(typePlaying)]; }
        public MultimediaFile PrevMediaFile() { CurrentIndex = --CurrentIndex; return MediaFiles[CurrentIndex]; }

        public uint NextIndex(TypePlaying typePlaying = TypePlaying.Serially) { CurrentIndex = ++CurrentIndex; return CurrentIndex;  }

    }

    public enum TypePlaying { Serially, Random }


    public enum TypeContent
    {
        Adv,
        News,
        Photo,
        Statistics,
        Default
    }

    public enum DayOfWeek {Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday, All}
}
