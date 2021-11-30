using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TVTK.Entity;
using TVTK.Enums;
//using System.Windows.Forms;

namespace TVTK.Playlist
{
    public class PlayList
    {
        /// <summary>
        /// Списоки доступных файлов рекламного, новостного и тп контента.
        /// </summary>
        ObservableCollection<PlayListByTypeContent> FileList { get; set; }
        /// <summary>
        /// Включен ли плейлист
        /// </summary>
        public bool EnablePlylist { get; private set; }
        /// <summary>
        /// Активен ли сейчс плейлист
        /// </summary>
        public bool ActivePlylist { get; private set; }
        /// <summary>
        /// Отображаемое имя плейлиста
        /// </summary>
        public string NamePlalist { get; set; }
        /// <summary>
        /// Уникальный идентификатор плейлиста
        /// </summary>
        public uint IdPlalist { get; set; }
        /// <summary>
        /// Дни недели проигрывания плейлиста
        /// </summary>
        /// <param name="Name"></param>
        public Enums.DayOfWeek dayOfWeek { get; set; }
        /// <summary>
        /// Приоритет плейлиста
        /// </summary>
        /// <param name="Name"></param>
        public uint Priotity { get; set; }
        /// <summary>
        /// Запланированные дни проигрывания контента
        /// </summary>
        public ObservableCollection<Dates> Days { get; set; }
        /// <summary>
        /// Текущий проигрываемый плейлист по типу контента
        /// </summary>
        public PlayListByTypeContent CurrentFileList { get; set; }



        public PlayList(string Name)
        {
            NamePlalist = Name;
        }

      //  playList = GetContentLocal(Properties.Settings.Default.LocalPathAdv, lvFiles);
      //  playListNews = GetContentLocal(Properties.Settings.Default.LocalPathNews, lvNews);


        public void NewPlaylist(List<Uri> uris) 
        {

        }
     
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

        public void SetTimeOfPlaying(System.DayOfWeek[] dayOfWeeks, List<TimeOfPlaying> times) { }

        public void SetTypeContent(TypeContent typeContent = TypeContent.Default) { }

        /// <summary>
        /// Получение следующего файла по текущему типу плейлиста
        /// </summary>
        /// <returns></returns>
        public MediaFile NextMediaFile() 
        {
            try
            {
                return CurrentFileList.Next();
            }
            catch (Exception)
            {

                throw;
            }          
        }

        /// <summary>
        /// Получение предыдущего файла по текущему типу плейлиста
        /// </summary>
        /// <returns></returns>
        public MediaFile PrevMediaFile() 
        {
            try
            {
                return CurrentFileList.Prev();
            }
            catch (Exception)
            {

                throw;
            }         
        }

        /// <summary>
        /// Получение текущего файла по текущему типу плейлиста
        /// </summary>
        /// <returns></returns>
        public MediaFile CurrentFile()
        {
            try
            {
                return CurrentFileList.Current();
            }
            catch (Exception)
            {

                throw;
            }       
        }

        /// <summary>
        /// Получение индекса предыдущего файла по текущему типу плейлиста
        /// </summary>
        /// <returns></returns>
        public uint PrevIndex() 
        {
            try
            {
                return CurrentFileList.PrevIndex();
            }
            catch (Exception)
            {

                throw;
            }            
        }
        /// <summary>
        /// Получение индекса следующего файла по текущему типу плейлиста
        /// </summary>
        /// <returns></returns>
        public uint NextIndex() 
        {
            try
            {
                return CurrentFileList.NextIndex();
            }
            catch (Exception)
            {

                throw;
            }       
        }

        /// <summary>
        /// Получение индекса текущего файла по текущему типу плейлиста
        /// </summary>
        /// <returns></returns>
        public uint CurrentIndex() 
        {
            try
            {
                return CurrentFileList.CurrentIndex();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }          
        }
    }
}
