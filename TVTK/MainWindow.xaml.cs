using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Windows.Media.Animation;
using System.Threading;
using System.Windows.Threading;
using System.Security.Policy;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Net;
using System.Net.Sockets;
using System.Net.Http;
using System.Web.Helpers;
using Newtonsoft.Json;
using System.Text.Encodings.Web;
using System.Web;

namespace TVTK
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        MediaElement mediaElement; // Проигрыватель видео и рекламы
        List<Uri> playList; // плейлист рекламы
        List<Uri> playListNews; // плейлист новостей.
        int queue; // текущее значение индекса плейлиста
        DispatcherTimer timer;//Таймер для проверки текущего времени и запуска/остановки проигрывания всего контента.
        DispatcherTimer timerStartNews;//Таймер для запуска показа новостей.
        DispatcherTimer timerEndNews;//Таймер для остановки показа новостей.
        bool playing = false; // маркер, позволяющий определить текущее состояние проигрывателя видео/рекламы
        bool showNews = false; //маркер, похволяющий определить текущее состояние показа новостей. Нужен для таймеров во избежание пвовторного запуска новостей.
        bool StartWithoutTime = false; // маркер, позволяющий определить тип запуска проигрывания. Варианты: проверять время и соблюдать время работы/ Проигрывать постоянно.
        Window window;//окно проигрывателя рекламы/видео
        TypeWork typeWork;
        List<List<MultimediaFile>> settingFromServer;


        public ObservableCollection<Time> viewModelTime //Коллекция времени работы плеера
        {
            get;
            set;
        }

        public MainWindow()
        {
            InitializeComponent();
            tbxHeight.Text = Properties.Settings.Default.Height.ToString();
            tbxWidth.Text = Properties.Settings.Default.Width.ToString();
            typeWork = (TypeWork)Properties.Settings.Default.TypeWork;
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0,1,0); //Таймер проверяет время каждую минуту           
            if (Properties.Settings.Default.PeriodNews>0)
            {
                timerStartNews = new DispatcherTimer();
                timerStartNews.Interval = new TimeSpan(0, (int)Properties.Settings.Default.PeriodNews, 0);
                if (Properties.Settings.Default.DurationNews > 0)
                {
                    timerEndNews = new DispatcherTimer();
                    timerEndNews.Interval = new TimeSpan(0, (int)Properties.Settings.Default.DurationNews, 0);
                }
            }
            chbNews.IsChecked = Properties.Settings.Default.News;
            tbxIPServer.Text = Properties.Settings.Default.AdressServer;
            tbxIPServer_Port.Text = Properties.Settings.Default.PortServer.ToString();
            tbxNameClient.Text = Properties.Settings.Default.NameClient;
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.LocalPathAdv))
            {
                Properties.Settings.Default.LocalPathAdv = Directory.GetCurrentDirectory();
            }
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.LocalPathNews))
            {
                Properties.Settings.Default.LocalPathNews = Directory.GetCurrentDirectory();
            }
            tbxPathAdv.Text = Properties.Settings.Default.LocalPathAdv;
            tbxPathNews.Text = Properties.Settings.Default.LocalPathNews;
            tbxPeriodNews.Text = Properties.Settings.Default.PeriodNews.ToString(); ;
            tbxDurationNews.Text = Properties.Settings.Default.DurationNews.ToString();
            switch (typeWork)
            {
                case (TypeWork)TypeWork.Local: 
                    rbLocal.IsChecked = true;
                    break;
                case (TypeWork)TypeWork.Mixed:
                    rbMixed.IsChecked = true;
                    break;
                case (TypeWork)TypeWork.Network:
                    rbNetwork.IsChecked = true;
                    break;
            }

            if (Properties.Settings.Default.Time == null) //первичная инициализация параметра времени работы. Если он пустой, то надо создать.
            {
                Properties.Settings.Default.Time = new List<Time>();
            }

            viewModelTime = new ObservableCollection<Time>(Properties.Settings.Default.Time);

            viewModelTime.CollectionChanged += ViewModelTime_CollectionChanged;

            if (viewModelTime.Count == 0)//если в параметре времени работы нет параметров,то надо их инициализировать дефолтными значениями
            {
                viewModelTime.Add(new Time { From = "08:00", Before = "09:00" });
                viewModelTime.Add(new Time { From = "11:30", Before = "13:00" });
                viewModelTime.Add(new Time { From = "16:00", Before = "17:00" });
            }

            dgTime.ItemsSource = viewModelTime;            
        }

        private void ViewModelTime_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            
            Properties.Settings.Default.Time = viewModelTime.ToList<Time>();
            Properties.Settings.Default.Save();
        }

        private void ButtonWithoutTimer_Click(object sender, RoutedEventArgs e)//Запуск проигрывателя без проверки времени.
        {
            timer.Stop();
            StartWithoutTime = true;
            timer.Tick -= new EventHandler(CheckPlayer);
            StartPlayer();
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)//Запуск проигрывателя с проверкой времени работы
        {
            StartWithoutTime = false;
            timer.Tick += new EventHandler(CheckPlayer);
            StartPlayer();
            timer.Start();
        }

        public void CheckPlayer(object sender, EventArgs e) //Запуск плеера если наступило рабочее время.
        {
            if (CheckTime() == true && playing == false && StartWithoutTime == false)
            {
                mediaElement.Play();
            }
        }

        public bool CheckTime() //Проверка времени работы. Происходит каждую минуту по таймеру.
        {
            TimeSpan timeSpanFrom;
            TimeSpan timeSpanBefore;
            TimeSpan dateTime = TimeSpan.Parse(DateTime.Now.ToString("HH:mm"));

            foreach (var item in viewModelTime)
            {
                timeSpanFrom = TimeSpan.Parse(item.From);
                timeSpanBefore = TimeSpan.Parse(item.Before);

                 if (dateTime >= timeSpanFrom && dateTime <= timeSpanBefore)
                {
                    return true;
                }
            }
            return false;
        }


        public async void StartPlayer() //Создание окна проигрывателя и его запуск.
        {
            queue = 1;

            if (Properties.Settings.Default.News)
            {
                timerStartNews.Tick += new EventHandler(StartNews);
                timer.Start();
            }
            

            switch (Properties.Settings.Default.TypeWork)
            {
                case (uint)TypeWork.Local:
                    if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.LocalPathAdv))
                    {
                        playList = GetContentLocal(Properties.Settings.Default.LocalPathAdv);
                        playListNews = GetContentLocal(Properties.Settings.Default.LocalPathNews);
                        CreatePlayer();
                        //  UriBuilder uriBuilder = new UriBuilder(@"I:\Videos\Безумный макс.mkv");                                 
                    }
                    else
                    {
                        MessageBox.Show("Необходимо задать путь к контенту.");
                    }
                    break;
                case (uint)TypeWork.Network:
                    settingFromServer = await GetPropertiesFromServer();
                    playList = GetContentLan(settingFromServer[(int)Type.Adv]);
                    playListNews = GetContentLan(settingFromServer[(int)Type.News]);
                    CreatePlayer();
                    break;
                case (uint)TypeWork.Mixed:
                    break;
            }

            if (playList != null)
            {
                mediaElement.Source = playList.FirstOrDefault();
                queue++;
                if (StartWithoutTime)
                {
                    mediaElement.Play();
                }

                // CompositionTarget.Rendering += CompositionTarget_Rendering;
               // timer.Tick += new EventHandler(NewsStart);
            }
            else
            {
                MessageBox.Show("Плейлист пуст.");
            }
           
        }

        public void StartNews(object sender, EventArgs e) 
        {
            if (!showNews)
            {
                timerStartNews.Stop();
                timerEndNews.Tick += new EventHandler(StopNews);
                timerEndNews.Start();
              //  CreateNewWindow(window.Content as Canvas);
                showNews = true;                
                AnimationNews();
            }
        }

        public void StopNews(object sender, EventArgs e) 
        {
            if (showNews)
            {
                timerStartNews.Start();
                timerEndNews.Tick -= new EventHandler(StopNews);
                timerEndNews.Stop();
                showNews = false;
                AnimationNews();
            }
        }

        public void CreatePlayer() 
        {
            mediaElement = new MediaElement();
            Thickness thickness = new Thickness(0);
            mediaElement.Margin = thickness;

            mediaElement.KeyDown += MediaElement_KeyDown;
            mediaElement.MediaOpened += MediaElement_MediaOpened;
            mediaElement.MediaEnded += new RoutedEventHandler(mediaElement_MediaEnded);

            window = null;
            window = new Window();
            window.Width = Properties.Settings.Default.Width; // System.Windows.SystemParameters.FullPrimaryScreenWidth;
            window.Height = Properties.Settings.Default.Height; // System.Windows.SystemParameters.FullPrimaryScreenHeight;
            Canvas canvas = new Canvas();
            canvas.Width = window.Width;
            canvas.Height = window.Height;
            window.Content = canvas;
            canvas.Children.Add(mediaElement);
            //  window.Content = mediaElement;
            window.KeyDown += window_KeyDown;
            window.Cursor = Cursors.None;
            window.Show();
            if (Properties.Settings.Default.News)
            {
                CreateNewWindow(canvas);
            }
            window.Activate();
            window.Left = 0;
            window.Top = 0;
            window.WindowStyle = WindowStyle.None;
            window.ResizeMode = ResizeMode.NoResize;
            window.Background = new SolidColorBrush(Colors.Black);

            //  mediaElement.Style.Setters.Add(new Setter { Property = Control.BackgroundProperty, Value = new SolidColorBrush(Colors.Black) });

            // mediaElement.MinWidth = Convert.ToInt32(tbxWidth.Text) * (Convert.ToInt32(tbxMonitors.Text) / 2);
            // mediaElement.MinHeight = Convert.ToInt32(tbxHeight.Text) * (Convert.ToInt32(tbxMonitors.Text) / 2);
            mediaElement.Height = window.Height;
            mediaElement.Width = window.Width;
            mediaElement.Stretch = Stretch.UniformToFill;
            mediaElement.LoadedBehavior = MediaState.Manual;
            mediaElement.UnloadedBehavior = MediaState.Manual;
        }

        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            playing = true;
        }//Выполняется при открытии файла плеером.

        void mediaElement_MediaEnded(object sender, RoutedEventArgs e)//Выполняется при окончаниипроигрывания ролика
        {           
            mediaElement.Close();
            playing = false;
            if (CheckTime() || StartWithoutTime)
            {
                if (queue > playList.Count)
                {
                    queue = 1;
                }
                mediaElement.Source = playList[queue - 1];
                mediaElement.Position = TimeSpan.FromSeconds(0);
                mediaElement.Play();
                queue++;
            }
        }

        public List<Uri> GetContentLocal(string path) //Получает список файлов для проигрывания с указанным расширением и только в открытой директории. Подкаталоги пока еще не смотрит. Передает коллекцию в плелист
        {
            var temp = new List<Uri>();
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            foreach (var item in directoryInfo.GetFiles())
            {
                if (item.Extension.ToLower() == ".mkv" || item.Extension.ToLower() == ".mp4" || item.Extension.ToLower() == ".avi"  || item.Extension.ToLower() == ".jpg" || item.Extension.ToLower() == ".mov")
                {
                    temp.Add(new UriBuilder(item.FullName).Uri);
                }            
            }
            lvFiles.ItemsSource = null;
            lvFiles.Items.Clear();
            lvFiles.ItemsSource = temp;
            return temp;
        }

        public List<Uri> GetContentLan(List<MultimediaFile> multimediaFiles)//List<List<MultimediaFile>> multimediaFiles) //нужно разделить на разные плейлисты.
        {
            var Temp = new List<Uri>();
            foreach (var item in multimediaFiles)
            {
                Temp.Add(new UriBuilder("http", Properties.Settings.Default.AdressServer, (int)Properties.Settings.Default.PortServer, "/getmultimedia", "?stream=" + Properties.Settings.Default.Stream + "&content=" + (Type)item.type + "&id=" + item.id).Uri);              
            }
            
            return Temp;
        }

        public async Task<List<List<MultimediaFile>>> GetPropertiesFromServer() 
        {
            var addressServer = new UriBuilder("http", Properties.Settings.Default.AdressServer, (int)Properties.Settings.Default.PortServer).Uri;
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
                var obj = JsonConvert.DeserializeObject<List<List<MultimediaFile>>>(product);
                return obj;//  playList = GetVideosLan(obj);
            }
            else
            {
                MessageBox.Show("Нет ответа от сервера.");
                return null;
            }
        }

        private void MediaElement_KeyDown(object sender, KeyEventArgs e)
        {
            var temp = sender as MediaElement;
            if (e.Key == Key.Escape)
            {
                (sender as MediaElement).Stop();
                (sender as MediaElement).Close();
                this.Close();

            }
        }

        private void window_KeyDown(object sender, KeyEventArgs e)//управление проигрыванием плеер и новостей
        {
            

            switch (e.Key)
            {                           
                case Key.Pause:                   
                    mediaElement.Pause();
                    break;           
                case Key.Escape:
                    timer.Tick -= new EventHandler(CheckPlayer);
                    var temp = (sender as Window).Content as Canvas;
                    (temp.Children[0] as MediaElement)?.Close();
                    (sender as Window).Close();
                    playList.Clear();
                    playing = false;
                    break;              
                case Key.Space:
                    mediaElement.Pause();
                    break;
                case Key.Enter:
                    mediaElement.Play();
                    break;
                case Key.Left:
                    mediaElement.Position = mediaElement.Position - new TimeSpan(0,0,0,10);
                    break;
                case Key.Up:
                    mediaElement.Volume += 0.05D; 
                    break;
                case Key.Right:
                    var t = mediaElement.Position + new TimeSpan(0, 0, 0, 10);
                    if (t >= mediaElement.NaturalDuration.TimeSpan)
                    {
                        t = mediaElement.NaturalDuration.TimeSpan;
                    }
                    mediaElement.Position = t;
                    break;
                case Key.Down:
                    mediaElement.Volume -= 0.05D;
                    break;                       
            }
        }

        private void btnAddTime_Click(object sender, RoutedEventArgs e)//Добавление времени работы
        {
            var time = new Time();

            time.Before = tbxBreakBefore.Text;
            time.From = tbxBreakFrom.Text; 
            viewModelTime.Add(time);

        }
        
        private void btnDelTime_Click(object sender, RoutedEventArgs e)//Удаление выделенного времени работы
        {
            Time t = dgTime.SelectedItem as Time;
            viewModelTime.Remove(t);
        }

        static void CreateNewWindow(Canvas canvas)//Создание окна новостей
        {
            StackPanel contentControlNews = new StackPanel();

            contentControlNews.Width = canvas.Width / 2;
            contentControlNews.Height = canvas.Height / 2;

            //contentControlNews.Background = new SolidColorBrush(Colors.White);
             contentControlNews.Background = new SolidColorBrush(Colors.Transparent);
            //  ImageBrush imageBrush = new ImageBrush(new BitmapImage(new Uri(@".\TKS.png", UriKind.Relative)));
            //   contentControlNews.Background = imageBrush;
            MediaElement mediaElementNews = new MediaElement();
            mediaElementNews.Source = new Uri(@".\TKS.png", UriKind.Relative);
            mediaElementNews.Width = contentControlNews.Width;
            mediaElementNews.Height = contentControlNews.Height;
            mediaElementNews.MediaOpened += MediaElementNews_MediaOpened;
            mediaElementNews.MediaEnded += MediaElementNews_MediaEnded; ;
            mediaElementNews.KeyDown += MediaElementNews_KeyDown;              
            contentControlNews.Children.Add(mediaElementNews);
            contentControlNews.Margin = new Thickness(canvas.Width, canvas.Height/2, -contentControlNews.Width, 0);
            canvas.Children.Add(contentControlNews);
            canvas.Children[1].Visibility = Visibility.Visible;

        }

        private static void MediaElementNews_MediaOpened(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void MediaElementNews_MediaEnded(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void MediaElementNews_KeyDown(object sender, KeyEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void AnimationNews()//Выезд и уход за экран окна новостей. Проверяет текущее состояние новостей и выполняет требуемые действияю
        {
            Canvas canvas = window.Content as Canvas;
            StackPanel contentControlNews = canvas.Children[1] as StackPanel;
            ThicknessAnimation thicknessAnimation = new ThicknessAnimation();

            if (showNews == false)
            {
                thicknessAnimation.From = contentControlNews.Margin;
                thicknessAnimation.To = new Thickness(canvas.Width / 2, canvas.Height / 2, 0, 0);
                showNews = true;
            }
            else
            {
                thicknessAnimation.From = contentControlNews.Margin;
                thicknessAnimation.To = new Thickness(canvas.Width, canvas.Height / 2, 0, 0);
                showNews = false;
            }
            

            thicknessAnimation.Duration = TimeSpan.FromSeconds(5);
            contentControlNews.BeginAnimation(StackPanel.MarginProperty, thicknessAnimation);
         
        }

        private void btnApplySetting_Click(object sender, RoutedEventArgs e)
        {
            uint i, b;
            bool settings = true;
            string error = "Проверьте следующие настройки:";
            if (uint.TryParse(tbxHeight.Text, out i) && uint.TryParse(tbxWidth.Text,out b))
            {
                Properties.Settings.Default.Height = i;
                Properties.Settings.Default.Width = b;
                typeWork = (TypeWork)Properties.Settings.Default.TypeWork;

                if (typeWork == TypeWork.Mixed || typeWork == TypeWork.Network)
                {

                    if (string.IsNullOrWhiteSpace(tbxNameClient.Text))
                    {
                        settings = false;
                        error += "\nИмя клиента не должно быть пустым. Введите имя клиента";
                    }
                    else
                    {
                        Properties.Settings.Default.NameClient = tbxNameClient.Text;
                    }
                    if (string.IsNullOrWhiteSpace(tbxIPServer.Text) && string.IsNullOrWhiteSpace(tbxIPServer_Port.Text))
                    {
                        settings = false;
                        error += "\nАдрес сервера и порт не должен быть пустыми. Введите адрес сервера.";
                    }
                    else
                    {
                        //Regex regex = new Regex(@"\d{1,3}.\d{1,3}.\d{1,3}.\d{1,3}");
                        //regex.IsMatch(tbxIPServer.Text) &&
                        uint x;
                        if (uint.TryParse(tbxIPServer_Port.Text,out x))
                        {
                            Properties.Settings.Default.AdressServer = tbxIPServer.Text;
                            Properties.Settings.Default.PortServer = x;
                        }
                        else
                        {
                            settings = false;
                            error += "\nПорт должен состоять только из цифр. Введите адрес сервера и порт.";
                        }                     
                    }
                }
            }
            else
            {
                settings = false;
                error += "Настройки размеров экрана не применены. Вводить можно только цифры.";
            }

            uint p, d;
            if (uint.TryParse(tbxDurationNews.Text, out d) && uint.TryParse(tbxPeriodNews.Text, out p))
            {
                Properties.Settings.Default.DurationNews = d;
                Properties.Settings.Default.PeriodNews = p;
            }
            else
            {
                settings = false;
                error += "\nВремя должно состоять из цифр.";
            }
            if (settings)
            {
                Properties.Settings.Default.Save();
                MessageBox.Show("Настройки применены.");
            }
            else
            {
                MessageBox.Show(error);
            }
        }

        public enum TypeWork
        {
            Local,
            Network,
            Mixed
        }

        private void rbtypeWork_Checked(object sender, RoutedEventArgs e)
        {
            var rb = sender as RadioButton;
            switch ((TypeWork)Convert.ToInt32(rb.Tag))
            {
                case TypeWork.Local:
                    tbxIPServer.IsEnabled = false;
                    tbxNameClient.IsEnabled = false;
                    typeWork = TypeWork.Local;
                    break;
                case TypeWork.Network:
                    tbxIPServer.IsEnabled = true;
                    tbxNameClient.IsEnabled = true;
                    typeWork = TypeWork.Network;
                    break;
                case TypeWork.Mixed:
                    tbxIPServer.IsEnabled = true;
                    tbxNameClient.IsEnabled = true;
                    typeWork = TypeWork.Mixed;
                    break;
            }
        }

        private void btnSetPathNews_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.InitialDirectory = Directory.GetCurrentDirectory();
            dialog.Multiselect = false;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                tbxPathNews.Text = dialog.FileName;
                Properties.Settings.Default.LocalPathNews = dialog.FileName;
            }
        }

        private void btnSetPathAdv_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.InitialDirectory = Directory.GetCurrentDirectory();
            dialog.Multiselect = false;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                tbxPathAdv.Text = dialog.FileName;
                Properties.Settings.Default.LocalPathAdv = dialog.FileName;

            }
        }


    }
}
