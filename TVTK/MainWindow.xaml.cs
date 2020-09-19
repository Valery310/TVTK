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
using System.Media;
//using NAudio.CoreAudioApi;

namespace TVTK
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        MediaElement mediaElement; // Проигрыватель рекламы
        MediaElement mediaElementNews;// Проигрыватель новостей
        List<Uri> playList; // плейлист рекламы
        List<Uri> playListNews; // плейлист новостей.
        int queue; // текущее значение индекса плейлиста рекламных роликов
        int queueNews;// текущее значение индекса плейлиста новостей
        DispatcherTimer timer;//Таймер для проверки текущего времени и запуска/остановки проигрывания всего контента.
        DispatcherTimer timerStartNews;//Таймер для запуска показа новостей.
        DispatcherTimer timerEndNews;//Таймер для остановки показа новостей.
        DispatcherTimer timerDurationShowNews;//таймер времени показа одной новости.
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

        public ObservableCollection<TV> viewModelTV //Коллекция телевизоров
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

            timerDurationShowNews = new DispatcherTimer();
            timerDurationShowNews.Interval = new TimeSpan(0, 0, Properties.Settings.Default.DurationShowNews);
            timerDurationShowNews.Tick += Timer_Tick;
            tbxPositionX.Text = Properties.Settings.Default.PositionX.ToString();
            tbxPositionY.Text = Properties.Settings.Default.PositionY.ToString();
        



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
            chkbxAutoplay.IsChecked = Properties.Settings.Default.Autoplay;
            tbxTitlePlayer.Text = Properties.Settings.Default.TitlePlayer;
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

            if (Properties.Settings.Default.TVs == null) //первичная инициализация параметра времени работы. Если он пустой, то надо создать.
            {
                Properties.Settings.Default.TVs = new List<TV>();
            }

            viewModelTV = new ObservableCollection<TV>(Properties.Settings.Default.TVs);
            foreach (var item in viewModelTV)
            {
                item.Status = "-";
            }
            viewModelTV.CollectionChanged += ViewModelTV_CollectionChanged;

            viewModelTime = new ObservableCollection<Time>(Properties.Settings.Default.Time);

            viewModelTime.CollectionChanged += ViewModelTime_CollectionChanged;

            if (viewModelTime.Count == 0)//если в параметре времени работы нет параметров,то надо их инициализировать дефолтными значениями
            {
                viewModelTime.Add(new Time { From = "08:00", Before = "09:00" });
                viewModelTime.Add(new Time { From = "11:30", Before = "13:00" });
                viewModelTime.Add(new Time { From = "16:00", Before = "17:00" });
            }

            AudioOutput.GetAudio();

            dgTime.ItemsSource = viewModelTime;
            dgTV.ItemsSource = viewModelTV;

            if (Properties.Settings.Default.Autoplay == true)
            {
                ButtonStart_Click(new object(), new RoutedEventArgs());
                this.WindowState = WindowState.Minimized;
               //window.Activate();
            }
        }

        private void ViewModelTV_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Properties.Settings.Default.TVs = viewModelTV.ToList<TV>();
            Properties.Settings.Default.Save();
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
            //TV.WOL(viewModelTV.ToList<TV>());
            StartWithoutTime = false;
            timer.Tick += new EventHandler(CheckPlayer);           
            StartPlayer();
            timer.Start();
           // CheckPlayer(new object(), new EventArgs());
        }

        public void CheckPlayer(object sender, EventArgs e) //Запуск плеера если наступило рабочее время.
        {
            if (CheckTime() == true && playing == false && StartWithoutTime == false && !showNews)
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
            queueNews = 1;

            if (Properties.Settings.Default.News)
            {
                timerStartNews.Tick += new EventHandler(StartNews);
                timerStartNews.Start();
                timer.Start();
            }
            

            switch (Properties.Settings.Default.TypeWork)
            {
                case (uint)TypeWork.Local:
                    if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.LocalPathAdv))
                    {
                        playList = GetContentLocal(Properties.Settings.Default.LocalPathAdv, lvFiles);
                        playListNews = GetContentLocal(Properties.Settings.Default.LocalPathNews, lvNews);
                        CreatePlayer();
                        //  UriBuilder uriBuilder = new UriBuilder(@"I:\Videos\Безумный макс.mkv");                                 
                    }
                    else
                    {
                        MessageBox.Show("Необходимо задать путь к контенту.");
                    }
                    break;
                case (uint)TypeWork.Network:
                    try
                    {
                        settingFromServer = await GetPropertiesFromServer();
                        playList = GetContentLan(settingFromServer[(int)Type.Adv]);
                        playListNews = GetContentLan(settingFromServer[(int)Type.News]);
                        CreatePlayer();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    
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
            if (!showNews && CheckTime())
            {
                timerStartNews.Stop();
                timerEndNews.Tick += new EventHandler(StopNews);
                timerEndNews.Start();
                //  CreateNewWindow(window.Content as Canvas);
                mediaElement.Pause();
                mediaElementNews.Source = playListNews.FirstOrDefault();
                mediaElementNews.Play();
                queueNews++;
                showNews = true;                
                AnimationNews();
                mediaElementNews.Focus();
            }
        }

        public void StopNews(object sender, EventArgs e) 
        {
            if (showNews)
            {
                timerStartNews.Start();
                timerEndNews.Tick -= new EventHandler(StopNews);
                timerEndNews.Stop();
                mediaElement.Play();
                mediaElementNews.Pause();
                showNews = false;
                AnimationNews();
                mediaElement.Focus();
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
         //Попытка сделать выбор аудиовыхода   mediaElement.Audi

            window = null;
            window = new Window();
            window.Title = Properties.Settings.Default.TitlePlayer;
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
                CreateNewsWindow(canvas);
            }
            window.Activate();
            window.Left = Properties.Settings.Default.PositionX;
            window.Top = Properties.Settings.Default.PositionY;
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

        public List<Uri> GetContentLocal(string path, ListView lvPlaylist) //Получает список файлов для проигрывания с указанным расширением и только в открытой директории. Подкаталоги пока еще не смотрит. Передает коллекцию в плелист
        {
            var temp = new List<Uri>();
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            foreach (var item in directoryInfo.GetFiles())
            {
                if (item.Extension.ToLower() == ".mkv" || item.Extension.ToLower() == ".mp4" || item.Extension.ToLower() == ".avi"  || item.Extension.ToLower() == ".jpg" || item.Extension.ToLower() == ".mov")
                {
                    temp.Add(new Uri(item.FullName, UriKind.Absolute));
                }            
            }
            lvPlaylist.ItemsSource = null;
            lvPlaylist.Items.Clear();
            lvPlaylist.ItemsSource = temp;
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
                    if (!showNews)
                    {
                        mediaElement.Pause();
                    }
                    else
                    {
                        mediaElementNews.Pause();
                    }
                    break;           
                case Key.Escape:
                    timer.Tick -= new EventHandler(CheckPlayer);
                    var temp = (sender as Window).Content as Canvas;
                    (temp.Children[0] as MediaElement)?.Close();
                    mediaElementNews.Close();
                    (sender as Window).Close();
                    playList.Clear();
                    playListNews.Clear();
                    showNews = false;
                    playing = false;
                    break;              
                case Key.Space:
                    if (!showNews)
                    {
                        mediaElement.Pause();
                    }
                    else
                    {
                        mediaElementNews.Pause();
                    }
                    break;
                case Key.Enter:
                    if (!showNews)
                    {
                        mediaElement.Play();
                    }
                    else
                    {
                        mediaElementNews.Play();
                    }                    
                    break;
                case Key.Left:
                    if (!showNews)
                    {
                        mediaElement.Position = mediaElement.Position - new TimeSpan(0, 0, 0, 10);
                    }
                    else
                    {
                        mediaElementNews.Position = mediaElement.Position - new TimeSpan(0, 0, 0, 10);
                    }                    
                    break;
                case Key.Up:
                    if (!showNews)
                    {
                        mediaElement.Volume += 0.05D;
                    }
                    else
                    {
                        mediaElementNews.Volume += 0.05D;
                    }                   
                    break;
                case Key.Right:
                    if (!showNews)
                    {
                        var t = mediaElement.Position + new TimeSpan(0, 0, 0, 10);
                        if (t >= mediaElement.NaturalDuration.TimeSpan)
                        {
                            t = mediaElement.NaturalDuration.TimeSpan;
                        }
                        mediaElement.Position = t;
                    }
                    else
                    {
                        var t = mediaElementNews.Position + new TimeSpan(0, 0, 0, 10);
                        if (t >= mediaElementNews.NaturalDuration.TimeSpan)
                        {
                            t = mediaElementNews.NaturalDuration.TimeSpan;
                        }
                        mediaElementNews.Position = t;
                    }                    
                    break;
                case Key.Down:
                    if (!showNews)
                    {
                        mediaElement.Volume -= 0.05D;
                    }
                    else
                    {
                        mediaElementNews.Volume -= 0.05D;
                    }                
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

        void CreateNewsWindow(Canvas canvas)//Создание окна новостей
        {
            StackPanel contentControlNews = new StackPanel();

            contentControlNews.Width = canvas.Width;
            contentControlNews.Height = canvas.Height;

            //contentControlNews.Background = new SolidColorBrush(Colors.White);
             contentControlNews.Background = new SolidColorBrush(Colors.Transparent);
            //  ImageBrush imageBrush = new ImageBrush(new BitmapImage(new Uri(@".\TKS.png", UriKind.Relative)));
            //   contentControlNews.Background = imageBrush;
            mediaElementNews = new MediaElement();
            mediaElementNews.LoadedBehavior = MediaState.Manual;
            mediaElementNews.UnloadedBehavior = MediaState.Manual;
            mediaElementNews.Width = contentControlNews.Width;
            mediaElementNews.Height = contentControlNews.Height;
            mediaElementNews.MediaOpened += MediaElementNews_MediaOpened;
            mediaElementNews.MediaEnded += MediaElementNews_MediaEnded; ;
            mediaElementNews.KeyDown += MediaElementNews_KeyDown;
            mediaElementNews.Stretch = Stretch.UniformToFill;
          //  mediaElementNews.SpeedRatio = 0.80;
            contentControlNews.Children.Add(mediaElementNews);
            contentControlNews.Margin = new Thickness(canvas.Width/2, canvas.Height, -contentControlNews.Width, 0);
            canvas.Children.Add(contentControlNews);
            canvas.Children[1].Visibility = Visibility.Visible;
            mediaElementNews.Visibility = Visibility.Visible;
            contentControlNews.Visibility = Visibility.Visible;
            contentControlNews.KeyDown += ContentControlNews_KeyDown;
            canvas.KeyDown += Canvas_KeyDown;
            //mediaElementNews.Clock.
            //mediaElementNews.Focus();
            
        }

        private void ContentControlNews_KeyDown(object sender, KeyEventArgs e)//Надо определить какой из элементов будет реагировать на нажатие клавиш.
        {
            throw new NotImplementedException();
        }

        private void Canvas_KeyDown(object sender, KeyEventArgs e)//Надо определить какой из элементов будет реагировать на нажатие клавиш.
        {
            throw new NotImplementedException();
        }

        private void MediaElementNews_KeyDown(object sender, KeyEventArgs e)//Надо определить какой из элементов будет реагировать на нажатие клавиш.
        {
            throw new NotImplementedException();
        }

        private void MediaElementNews_MediaOpened(object sender, RoutedEventArgs e)
        {
            //вероятно, потребуется какя-то проверка времени при старте.
        }

        private void MediaElementNews_MediaEnded(object sender, RoutedEventArgs e)
        {
            mediaElementNews.Pause();       
            timerDurationShowNews.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            mediaElementNews.Close();
            if ((CheckTime() || StartWithoutTime) && showNews)
            {
                if (queueNews > playListNews.Count)
                {
                    queueNews = 1;
                }

                mediaElementNews.Source = playListNews[queueNews - 1];
                mediaElementNews.Position = TimeSpan.FromSeconds(0);
                mediaElementNews.Play();
                queueNews++;
            }
            (sender as DispatcherTimer).Stop();
        }

        public void AnimationNews()//Выезд и уход за экран окна новостей. Проверяет текущее состояние новостей и выполняет требуемые действияю
        {
            Canvas canvas = window.Content as Canvas;
            StackPanel contentControlNews = canvas.Children[1] as StackPanel;
            ThicknessAnimation thicknessAnimation = new ThicknessAnimation();
            thicknessAnimation.DecelerationRatio = 0.5;

            if (showNews == true)//если истина, то выводим экран новостей
            {
                thicknessAnimation.From = contentControlNews.Margin;
                thicknessAnimation.To = mediaElement.Margin;// new Thickness(canvas.ma, canvas.Height / 2, 0, 0);
              //  showNews = true;
            }
            else
            {
                thicknessAnimation.From = contentControlNews.Margin;
                thicknessAnimation.To = new Thickness(mediaElement.Width/2, mediaElement.Height, 0, 0);
              //  showNews = false;
            }
            

            thicknessAnimation.Duration = TimeSpan.FromSeconds(3);
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
                        uint port;
                        if (uint.TryParse(tbxIPServer_Port.Text,out port))
                        {
                            Properties.Settings.Default.AdressServer = tbxIPServer.Text;
                            Properties.Settings.Default.PortServer = port;
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
            int x, y;//проверка настроек изначального положения окна трансляции.
            if (int.TryParse(tbxPositionX.Text, out x) && int.TryParse(tbxPositionY.Text, out y))
            {
                Properties.Settings.Default.PositionX = x;
                Properties.Settings.Default.PositionY = y;
            }
            else
            {
                settings = false;
                error += "\nКординаты должны состоять из цифр.";
            }

            uint p, d;//проверка продолжительности рекламы и чсастоты ее вызова
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

            if (!string.IsNullOrWhiteSpace(tbxTitlePlayer.Text))
            {
                Properties.Settings.Default.TitlePlayer = tbxTitlePlayer.Text;
            }
            else
            {
                settings = false;
                error += "\nИмя плеера должно быть указано!.";
            }

            if (settings) //итоговая проверка. Сохранение или вывод всех ошибок.
            {
                Properties.Settings.Default.Autoplay = (bool)(chkbxAutoplay.IsChecked);
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
            Properties.Settings.Default.TypeWork = (uint)typeWork;
        }

        private void btnSetPathNews_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (!String.IsNullOrWhiteSpace(Properties.Settings.Default.LocalPathNews))
            {
                dialog.InitialDirectory = Properties.Settings.Default.LocalPathNews;
            }
            else
            {
                dialog.InitialDirectory = Directory.GetCurrentDirectory();
            }
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
            if (!String.IsNullOrWhiteSpace(Properties.Settings.Default.LocalPathAdv))
            {
                dialog.InitialDirectory = Properties.Settings.Default.LocalPathAdv;
            }
            else
            {
                dialog.InitialDirectory = Directory.GetCurrentDirectory();
            }      
            dialog.Multiselect = false;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                tbxPathAdv.Text = dialog.FileName;
                Properties.Settings.Default.LocalPathAdv = dialog.FileName;

            }
        }

        private void btnWOLTV_Click(object sender, RoutedEventArgs e)
        {
            TV.WOL(viewModelTV);
        }

        private void btnAddTV_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var tv = new TV(tbxNameTV.Text, tbxIPTV.Text, tbxMACTV.Text, tbxDescTV.Text);
                viewModelTV.Add(tv);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Введите данные правильно!\n" + ex); ;
            }
        }

        private void btnDelTV_Click(object sender, RoutedEventArgs e)
        {
            TV tv = dgTV.SelectedItem as TV;
            viewModelTV.Remove(tv);
        }

        private void tbxIPTV_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void tbxMACTV_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void tbxBreakBefore_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void tbxBreakFrom_TextChanged(object sender, TextChangedEventArgs e)
        {
         //   (sender as TextBox).Text = (sender as TextBox).Text.ToString("##:##");
           // (sender as TextBox).Text = Regex.Replace((sender as TextBox).Text, @"^(([0,1][0-9])|(2[0-3])):[0-5][0-9]", "");
        }

        private void tbxIPServer_TextChanged(object sender, TextChangedEventArgs e)
        {
          //  (sender as TextBox).Text = Regex.Replace((sender as TextBox).Text, @"((25[0-5]|2[0-4]\d|[01]?\d\d?)\.){3}(25[0-5]|2[0-4]\d|[01]?\d\d?)", "");
            //  e.Changes.
            //  (88005553535).ToString("+#-###-###-##-##") => +8 - 800 - 555 - 35 - 35
        }

        private void tbxWidth_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !(Char.IsDigit(e.Text, 0));
        }

        private void tbxMACTV_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !((Char.IsLetterOrDigit(e.Text, 0) || e.Text.ToCharArray()[0] == ':') && e.Text == " " && e.Text.Length<=95);
        }

        private void tbxIPTV_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !((Char.IsDigit(e.Text, 0) || e.Text.ToCharArray()[0] == '.') && e.Text.ToCharArray()[0] != ' ' && e.Text.ToCharArray().Length <= 15);
        }

        private void btnCheckStatus_Click(object sender, RoutedEventArgs e)
        {
            TV.CheckStatus(viewModelTV);
        }
    }
}
