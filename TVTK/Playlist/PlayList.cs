using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TVTK.Entity;
//using System.Windows.Forms;

namespace TVTK.Playlist
{
    public class PlayList
    {
        /// <summary>
        /// Список всех доступных файлов
        /// </summary>
        public FileList FileList { get; set; }
        /// <summary>
        /// Порядок воспроизведения, зависимый от ключа TypePlaying.
        /// </summary>
        public Dictionary<uint, uint> List { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public uint CurrentIndexMediaFile { get; private set; } // Индекс текущего проигрываемого файла
        /// <summary>
        /// 
        /// </summary>
        public bool EnablePlylist { get; private set; } // Включен ли плейлист
        /// <summary>
        /// 
        /// </summary>
        public bool ActivePlylist { get; private set; } //Активен ли сейчс плейлист
        /// <summary>
        /// Отображаемое имя плейлиста
        /// </summary>
        public string NamePlalist { get; set; }
        /// <summary>
        /// Уникальный идентификатор плейлиста
        /// </summary>
        public uint IdPlalist { get; set; }

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

        public void EditPositionFile(MediaFile multimediaFile, uint position) { }
       
        public void DeletePositionFile(MediaFile multimediaFile, uint position) { }
       
        public void AddPositionFile(MediaFile multimediaFile) { }

        public void SetDurationShow(uint minute = 0) { }
       
        public void SetFrequencyShow(uint minute = 0) { }

        public void SetTimeOfPlaying(DayOfWeek[] dayOfWeeks, List<TimeOfPlaying> times) { }

        public void SetTypeContent(TypeContent typeContent = TypeContent.Default) { }

        public MediaFile NextMediaFile() { return FileList[NextIndex()]; }
      
        public MediaFile PrevMediaFile() { return FileList[PrevIndex()]; }

        public uint PrevIndex() { CurrentIndexMediaFile = --CurrentIndexMediaFile; return CurrentIndexMediaFile; }

        public uint NextIndex() { CurrentIndexMediaFile = ++CurrentIndexMediaFile; return CurrentIndexMediaFile;  }

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
