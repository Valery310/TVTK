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
        int queue; // текущее значение индекса плейлиста
        DispatcherTimer timer;//Таймер для проверки текущего времени и запуска/остановки проигрывания видео/новостей.
        bool playing = false; // маркре, позволяющий определить текущее состояние проигрывателя видео/рекламы
        bool StartWithoutTime = false; // маркер, позволяющий определить тип запуска проигрывания. Варианты: проверять время и соблюдать время работы/ Проигрывать постоянно.
        Window window;//окно проигрывателя рекламы/видео
        bool showNews = false; //маркер, позволяющий понять текущее состояние показа новостей.

        public ObservableCollection<Time> viewModelTime //Коллекция времени работы плеера
        {
            get;
            set;
        }

        public MainWindow()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0,1,0); //Таймер проверяет время каждую минуту
            

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


        public void StartPlayer() //Создание окна проигрывателя и его запуск.
        {
            queue = 1;
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            //  dialog.InitialDirectory = currentDirectory;
            dialog.Multiselect = false;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                
                playList = GetVideos(dialog.FileName);
                mediaElement = new MediaElement();
                Thickness thickness = new Thickness(0);
                mediaElement.Margin = thickness;

                mediaElement.KeyDown += MediaElement_KeyDown;
                mediaElement.MediaOpened += MediaElement_MediaOpened;
                mediaElement.MediaEnded += new RoutedEventHandler(mediaElement_MediaEnded);

                window = null;
                window = new Window();
                window.Width = Convert.ToInt32(tbxWidth.Text) * (Convert.ToInt32(tbxMonitors.Text) / 2); // System.Windows.SystemParameters.FullPrimaryScreenWidth;
                window.Height = Convert.ToInt32(tbxHeight.Text) * (Convert.ToInt32(tbxMonitors.Text) / 2); // System.Windows.SystemParameters.FullPrimaryScreenHeight;
                Canvas canvas = new Canvas();
                canvas.Width = window.Width;
                canvas.Height = window.Height;
                window.Content = canvas;
                canvas.Children.Add(mediaElement);
              //  window.Content = mediaElement;
                window.KeyDown += window_KeyDown;
                window.Cursor = Cursors.None;
                window.Show();
                CreateNewWindow(canvas);
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



                //  UriBuilder uriBuilder = new UriBuilder(@"I:\Videos\Безумный макс.mkv");                  
                mediaElement.Source = playList.FirstOrDefault();
                queue++;
                if (StartWithoutTime)
                {
                    mediaElement.Play();
                }

                // CompositionTarget.Rendering += CompositionTarget_Rendering;
                timer.Tick += new EventHandler(NewsStart);
            }
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

        public List<Uri> GetVideos(string path) //Получает список файлов для проигрывания с указанным расширением и только в открытой директории. Подкаталоги пока еще не смотрит. Передает коллекцию в плелист
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
                    var temp = (sender as Window).Content;
                    (temp as MediaElement)?.Close();
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
            MediaElement mediaElement = new MediaElement();
            mediaElement.Source = new Uri(@".\TKS.png", UriKind.Relative);
            mediaElement.Width = contentControlNews.Width;
            mediaElement.Height = contentControlNews.Height;
            contentControlNews.Children.Add(mediaElement);
            contentControlNews.Margin = new Thickness(canvas.Width, canvas.Height/2, -contentControlNews.Width, 0);
            canvas.Children.Add(contentControlNews);
            canvas.Children[1].Visibility = Visibility.Visible;

        }

        public void NewsStart(object sender, EventArgs e)//Выезд и уход за экран окна новостей. Проверяет текущее состояние новостей и выполняет требуемые действияю
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
    }
}
