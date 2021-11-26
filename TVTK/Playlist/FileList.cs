using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TVTK.Entity;
using TVTK.Enums;
using NLog;
using System.Windows.Threading;

namespace TVTK.Playlist
{
    public class FileList
    {
        /// <summary>
        /// Директория с файлами для воспроизведения
        /// </summary>
        public string _Directory { get; private set; }
        /// <summary>
        /// Список полученных из дректории файлов
        /// </summary>
        public List<MediaFile> MediaFiles { get; private set; }
        /// <summary>
        /// Таймер обновления списка файлов из директории
        /// </summary>
        DispatcherTimer TimerRefresh;
        public static readonly NLog.Logger Logger = MainWindow.Logger;

        /// <summary>
        /// Получение файла по id из списка
        /// </summary>
        /// <param name="id">Уникальный идентификатор в списке файлов</param>
        /// <returns></returns>
        public MediaFile this[uint id]
        {
            get => GetElementById(id);
        }

        public FileList()
        {
            Logger.Trace("Создается список файлов");
            TimerRefresh = new DispatcherTimer();
            TimerRefresh
                .Interval = new TimeSpan(0, 1, 0);
        }

        /// <summary>
        /// Проверка существования файла в списке и в директории и запись результата в объект файла.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private MediaFile GetElementById(uint id) 
        {
            Logger.Trace($"Проверка существования файла с id: {id}");

            MediaFile mediaFile;

            if (MediaFiles != null && MediaFiles.Count > id && MediaFiles.ElementAt((int)id) != null)
            {
                mediaFile = MediaFiles.ElementAt((int)id);

                Logger.Trace($"Проверка существования файла в директории {mediaFile.PathToFile} с id: {id}");

                if (File.Exists(mediaFile.PathToFile.ToString()))
                {                    
                    Logger.Trace($"Проверка существования файла с id: {id} в директории {mediaFile.PathToFile} пройдена успешно. Файл: {mediaFile.NameOfFile}");
                    mediaFile.Exsist = true;
                    return mediaFile;
                }
                else
                {
                    Logger.Warn($"Проверка существования файла с id: {id} в директории {_Directory} не пройдена.");
                    mediaFile.Exsist = false;
                    return mediaFile;
                }
            }
            else
            {
                Logger.Warn($"Проверка существования файла с id: {id} в списке не пройдена.");

                mediaFile = new MediaFile(new Uri(""));
                mediaFile.Exsist = false;
                return mediaFile;
            }          
        }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="lvPlaylist"></param>
        private async void RefreshAllFilesAsync(ListView lvPlaylist) 
        {
            Logger.Trace("Начато обновление списка файлов");
            await Task.Run(() => { GetContentLocal(lvPlaylist); });
            Logger.Trace("Обновление успешно завершено");
        }

        /// <summary>
        /// Получает список файлов для проигрывания с указанным расширением. Передает коллекцию в визуальное представление списка файлов и сохзраняет коллекцию в списке файлов класса.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="lvPlaylist"></param>
        /// <returns></returns>
        public void GetContentLocal(ListView lvPlaylist) //
        {
            Logger.Trace("Начат поиск файлов");
            var temp = new List<MediaFile>();
            _Directory = Directory.Exists(_Directory) ? _Directory : Directory.GetCurrentDirectory();
            DirectoryInfo directoryInfo = new DirectoryInfo(_Directory);

            List<string> allowedExtensions = new List<string>();

            foreach (ExtensionsImageMediaFile item in Enum.GetValues(typeof(ExtensionsImageMediaFile)))
            {
                allowedExtensions.Add(item.ToString());
            }
            foreach (ExtensionsAudioMediaFile item in Enum.GetValues(typeof(ExtensionsAudioMediaFile)))
            {
                allowedExtensions.Add(item.ToString());
            }
            foreach (ExtensionsVideoMediaFile item in Enum.GetValues(typeof(ExtensionsVideoMediaFile)))
            {
                allowedExtensions.Add(item.ToString());
            }

            temp = GetAllFilesInDirectories(temp, directoryInfo, allowedExtensions);

            try
            {
                lvPlaylist.ItemsSource = null;
                lvPlaylist.Items.Clear();
                lvPlaylist.ItemsSource = temp;
                MediaFiles = temp;
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка чтения файлов: {ex.Message}");
               // MessageBox.Show($"Ошибка чтения файлов: {ex.Message}");
            }
 
        }

        /// <summary>
        /// Рекурсивно проходит по всем вложенным каталогам и добавляет в список файлы с поддерживаемым расширением.
        /// </summary>
        /// <param name="temp"></param>
        /// <param name="directoryInfo"></param>
        /// <param name="allowedExtensions"></param>
        /// <returns></returns>
        private List<MediaFile> GetAllFilesInDirectories(List<MediaFile> temp, DirectoryInfo directoryInfo, List<string> allowedExtensions) 
        {
            Logger.Error($"Начат поиск в {directoryInfo.FullName}");
            foreach (var item in directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).Where(file => allowedExtensions.Any(file.Extension.ToLower().EndsWith)))
            {
                temp.Add(new MediaFile(new Uri(item.FullName, UriKind.Absolute)));
            }

            foreach (var item in directoryInfo.GetDirectories())
            {
                temp = GetAllFilesInDirectories(temp, item, allowedExtensions);
            }
            Logger.Error($"Поиск в {directoryInfo.FullName} завершен.");
            return temp;
        }

        //    public List<Uri> GetContentLan(List<PlayList> playLists)//List<List<MultimediaFile>> multimediaFiles) //нужно разделить на разные плейлисты.
        //{
        //    var Temp = new List<Uri>();
        //    foreach (var item in playLists)
        //    {
        //        Temp.Add(new UriBuilder("http", Properties.Settings.Default.AdressServer, (int)Properties.Settings.Default.PortServer, "/getmultimedia", "?stream=" + Properties.Settings.Default.Stream).Uri);
        //    }

        //    return Temp;
        //}
    }

}
