using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using TVTK.Entity;
using TVTK.Enums;

namespace TVTK.Controller.Properties {
    /// <summary>
    /// Этот класс позволяет загружать, редактировать и сохранять параметры приложения в файл Settings.settings:
    /// </summary>
    public static partial class Settings {

        public static ObservableCollection<TimeOfPlaying> viewModelTime //Коллекция времени работы плеера
        {
            get;
            set;
        }

        public static ObservableCollection<TVWol> viewModelTV //Коллекция телевизоров
        {
            get;
            set;
        }

        public static TypeWork typeWork;

        public static MainWindow mainWindow { get; private set; }

        public static uint Height { get; private set; }
        
        public static uint Width { get; private set; }

        static Settings() {
            // // Для добавления обработчиков событий для сохранения и изменения параметров раскомментируйте приведенные ниже строки:
            //
            // this.SettingChanging += this.SettingChangingEventHandler;
            //
            // this.SettingsSaving += this.SettingsSavingEventHandler;
            //
        }

        /// <summary>
        /// Событие PropertyChanged возникает после изменения значения параметра.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void PropertyChangedEventHandler(object sender, System.Configuration.SettingChangingEventArgs e)
        {
            // Добавьте здесь код для обработки события SettingChangingEvent.
        }
        /// <summary>
        /// Событие SettingsLoaded возникает после загрузки значений параметров.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void SettingsLoadedEventHandler(object sender, System.Configuration.SettingChangingEventArgs e)
        {
            // Добавьте здесь код для обработки события SettingChangingEvent.
        }
        /// <summary>
        /// Событие SettingChanging возникает перед изменением значения параметра.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e) {
            // Добавьте здесь код для обработки события SettingChangingEvent.
        }

        /// <summary>
        /// Событие SettingsSaving возникает перед сохранением значений параметров.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e) {
            // Добавьте здесь код для обработки события SettingsSaving.
        }

        /// <summary>
        /// Загрузка настроек
        /// </summary>
        public static void Load(MainWindow main) 
        {
            mainWindow = main;

            mainWindow.tbxPositionX.Text = TVTK.Properties.Settings.Default.PositionX.ToString();
            mainWindow.tbxPositionY.Text = TVTK.Properties.Settings.Default.PositionY.ToString();
            mainWindow.chbNews.IsChecked = TVTK.Properties.Settings.Default.News;
            mainWindow.tbxIPServer.Text = TVTK.Properties.Settings.Default.AdressServer;
            mainWindow.tbxIPServer_Port.Text = TVTK.Properties.Settings.Default.PortServer.ToString();
            mainWindow.tbxNameClient.Text = TVTK.Properties.Settings.Default.NameClient;

            if (string.IsNullOrWhiteSpace(TVTK.Properties.Settings.Default.LocalPathAdv))
            {
                TVTK.Properties.Settings.Default.LocalPathAdv = Directory.GetCurrentDirectory();
            }
            if (string.IsNullOrWhiteSpace(TVTK.Properties.Settings.Default.LocalPathNews))
            {
                TVTK.Properties.Settings.Default.LocalPathNews = Directory.GetCurrentDirectory();
            }
            mainWindow.tbxPathAdv.Text = TVTK.Properties.Settings.Default.LocalPathAdv;
            mainWindow.tbxPathNews.Text = TVTK.Properties.Settings.Default.LocalPathNews;
            mainWindow.tbxPeriodNews.Text = TVTK.Properties.Settings.Default.PeriodNews.ToString(); ;
            mainWindow.tbxDurationNews.Text = TVTK.Properties.Settings.Default.DurationNews.ToString();
            mainWindow.chkbxAutoplay.IsChecked = TVTK.Properties.Settings.Default.Autoplay;
           // mainWindow.tbxTitlePlayer.Text = TVTK.Properties.Settings.Default.TitlePlayer;

            Height = TVTK.Properties.Settings.Default.Height;
            Width = TVTK.Properties.Settings.Default.Width;
            typeWork = (TypeWork)TVTK.Properties.Settings.Default.TypeWork;

            switch (typeWork)
            {
                case (TypeWork)TypeWork.Local:
                    mainWindow.rbLocal.IsChecked = true;
                    break;
                case (TypeWork)TypeWork.Mixed:
                    mainWindow.rbMixed.IsChecked = true;
                    break;
                case (TypeWork)TypeWork.Network:
                    mainWindow.rbNetwork.IsChecked = true;
                    break;
            }
            if (TVTK.Properties.Settings.Default.Time == null) //первичная инициализация параметра времени работы. Если он пустой, то надо создать.
            {
                TVTK.Properties.Settings.Default.Time = new List<TimeOfPlaying>();
            }

            if (TVTK.Properties.Settings.Default.TVs == null) //первичная инициализация параметра времени работы. Если он пустой, то надо создать.
            {
                TVTK.Properties.Settings.Default.TVs = new List<TVWol>();
            }

            viewModelTV = new ObservableCollection<TVWol>(TVTK.Properties.Settings.Default.TVs);
            foreach (var item in viewModelTV)
            {
                item.Status = "-";
            }
            viewModelTV.CollectionChanged += mainWindow.ViewModelTV_CollectionChanged;

            viewModelTime = new ObservableCollection<TimeOfPlaying>(TVTK.Properties.Settings.Default.Time);

            viewModelTime.CollectionChanged += mainWindow.ViewModelTime_CollectionChanged;

            if (viewModelTime.Count == 0)//если в параметре времени работы нет параметров,то надо их инициализировать дефолтными значениями
            {
                viewModelTime.Add(new TimeOfPlaying { From = DateTimeOffset.Parse("08:00"), Before = DateTimeOffset.Parse("09:00") });
                viewModelTime.Add(new TimeOfPlaying { From = DateTimeOffset.Parse("11:30"), Before = DateTimeOffset.Parse("13:00") });
                viewModelTime.Add(new TimeOfPlaying { From = DateTimeOffset.Parse("16:00"), Before = DateTimeOffset.Parse("17:00") });
            }

          //  mainWindow.chbScreenSaver.IsChecked = TVTK.Properties.Settings.Default.ScreenSaver;
          //  mainWindow.tbxDurationScreenSaver.Text = TVTK.Properties.Settings.Default.DurationScreenSaver.ToString();

            //   AudioOutput.GetAudio();

            mainWindow.dgTime.ItemsSource = viewModelTime;
            mainWindow.dgTV.ItemsSource = viewModelTV;

            if (TVTK.Properties.Settings.Default.Autoplay == true)
            {
                Scheduler.StartScheduler();
                mainWindow.WindowState = WindowState.Minimized;
            }
        }

        /// <summary>
        /// Загрузка плейлистов
        /// </summary>
        public static void LoadPlaylist() { }

        /// <summary>
        /// Сохранение плейлиста
        /// </summary>
        public static void SavePlaylist()
        {

        }

            /// <summary>
            /// Сохранение настроек
            /// </summary>
            public static void Save() 
        {
            //uint i, b;
            //bool settings = true;
            //string error = "Проверьте следующие настройки:";
            //if (uint.TryParse(tbxHeight.Text, out i) && uint.TryParse(tbxWidth.Text, out b))
            //{
            //    Properties.Settings.Default.Height = i;
            //    Properties.Settings.Default.Width = b;
            //    typeWork = (TypeWork)Properties.Settings.Default.TypeWork;

            //    if (typeWork == TypeWork.Mixed || typeWork == TypeWork.Network)
            //    {

            //        if (string.IsNullOrWhiteSpace(tbxNameClient.Text))
            //        {
            //            settings = false;
            //            error += "\nИмя клиента не должно быть пустым. Введите имя клиента";
            //        }
            //        else
            //        {
            //            Properties.Settings.Default.NameClient = tbxNameClient.Text;
            //        }
            //        if (string.IsNullOrWhiteSpace(tbxIPServer.Text) && string.IsNullOrWhiteSpace(tbxIPServer_Port.Text))
            //        {
            //            settings = false;
            //            error += "\nАдрес сервера и порт не должен быть пустыми. Введите адрес сервера.";
            //        }
            //        else
            //        {
            //            //Regex regex = new Regex(@"\d{1,3}.\d{1,3}.\d{1,3}.\d{1,3}");
            //            //regex.IsMatch(tbxIPServer.Text) &&
            //            uint port;
            //            if (uint.TryParse(tbxIPServer_Port.Text, out port))
            //            {
            //                Properties.Settings.Default.AdressServer = tbxIPServer.Text;
            //                Properties.Settings.Default.PortServer = port;
            //            }
            //            else
            //            {
            //                settings = false;
            //                error += "\nПорт должен состоять только из цифр. Введите адрес сервера и порт.";
            //            }
            //        }
            //    }
            //}
            //else
            //{
            //    settings = false;
            //    error += "Настройки размеров экрана не применены. Вводить можно только цифры.";
            //}
            //int x, y;//проверка настроек изначального положения окна трансляции.
            //if (int.TryParse(tbxPositionX.Text, out x) && int.TryParse(tbxPositionY.Text, out y))
            //{
            //    Properties.Settings.Default.PositionX = x;
            //    Properties.Settings.Default.PositionY = y;
            //}
            //else
            //{
            //    settings = false;
            //    error += "\nКординаты должны состоять из цифр.";
            //}

            //uint p, d;//проверка продолжительности рекламы и чсастоты ее вызова
            //if (uint.TryParse(tbxDurationNews.Text, out d) && uint.TryParse(tbxPeriodNews.Text, out p))
            //{
            //    Properties.Settings.Default.DurationNews = d;
            //    Properties.Settings.Default.PeriodNews = p;
            //}
            //else
            //{
            //    settings = false;
            //    error += "\nВремя должно состоять из цифр.";
            //}

            //if (!string.IsNullOrWhiteSpace(tbxTitlePlayer.Text))
            //{
            //    Properties.Settings.Default.TitlePlayer = tbxTitlePlayer.Text;
            //}
            //else
            //{
            //    settings = false;
            //    error += "\nИмя плеера должно быть указано!.";
            //}

            //uint temp = 0;
            //if (chbScreenSaver.IsChecked == true && uint.TryParse(tbxDurationScreenSaver.Text, out temp))
            //{
            //    Properties.Settings.Default.ScreenSaver = true;
            //    Properties.Settings.Default.DurationScreenSaver = temp;
            //}

            //if (settings) //итоговая проверка. Сохранение или вывод всех ошибок.
            //{
            //    Properties.Settings.Default.Autoplay = (bool)(chkbxAutoplay.IsChecked);
            //    Properties.Settings.Default.Save();
            //    MessageBox.Show("Настройки применены.");
            //}
            //else
            //{
            //    MessageBox.Show(error);
            //}
        }

        public static async Task<List<List<MediaFile>>> GetPropertiesFromServer()
        {
            var addressServer = new UriBuilder("http", TVTK.Properties.Settings.Default.AdressServer, (int)TVTK.Properties.Settings.Default.PortServer).Uri;
            //     var Stream = Properties.Settings.Default.Stream;

            var localAdress = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            var iPAddressLocal = localAdress.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork
            && !IPAddress.IsLoopback(ip)
            && !ip.ToString().StartsWith("169.254."));

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(addressServer + "getmultimedia");

            if (response.IsSuccessStatusCode)
            {
                var product = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<List<List<MediaFile>>>(product);
                return obj;//  playList = GetVideosLan(obj);
            }
            else
            {
                MessageBox.Show("Нет ответа от сервера.");
                return null;
            }
        }
    }
}
