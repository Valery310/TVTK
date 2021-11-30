using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TVTK.Entity;
using TVTK.Enums;

namespace TVTK.ViewModel
{
    /// <summary>
    /// Логика взаимодействия для TV.xaml
    /// </summary>
    public partial class TV : UserControl
    {
        MediaElement mediaElement; // Проигрыватель рекламы
        MediaElement mediaElementNews;// Проигрыватель новостей
     

        DispatcherTimer timerDurationShowNews;//таймер времени показа одной новости.
        bool playing = false; // маркер, позволяющий определить текущее состояние проигрывателя видео/рекламы
        bool showNews = false; //маркер, похволяющий определить текущее состояние показа новостей. Нужен для таймеров во избежание пвовторного запуска новостей.
        bool StartWithoutTime = false; // маркер, позволяющий определить тип запуска проигрывания. Варианты: проверять время и соблюдать время работы/ Проигрывать постоянно.
        Window window;//окно проигрывателя рекламы/видео
        
        List<List<MediaFile>> settingFromServer;
        ScreenSaver screenSaver;
        ScreenSaver screenSaver1;



        



        //public static Random GetThreadRandom()
        //{
        //    return random;
        //    // return random.Value;
        //}


        public TV(MainWindow mainWindow)
        {
            InitializeComponent();

           

            //timerDurationShowNews = new DispatcherTimer();
            //timerDurationShowNews.Interval = new TimeSpan(0, 0, Properties.Settings.Default.DurationShowNews);
            //timerDurationShowNews.Tick += Timer_Tick;
      
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

        private void Close() { }

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
                    TVTK.Controller.Scheduler.StopSheduler();
                    var temp = (sender as Window).Content as Canvas;
                    (temp.Children[0] as MediaElement)?.Close();
                    mediaElementNews.Close();
                    (sender as Window).Close();
                   // playList.Clear();
                  //  playListNews.Clear();
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
                        mediaElementNews.Position = mediaElementNews.Position - new TimeSpan(0, 0, 0, 10);
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
                case Key.N:
                    if (!showNews)
                    {
                        mediaElement_MediaEnded(mediaElement, new RoutedEventArgs());
                    }
                    else
                    {
                        MediaElementNews_MediaEnded(mediaElementNews, new RoutedEventArgs());
                    }
                    break;
            }
        }

        private void MediaElementNews_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (mediaElementNews.HasAudio == false)
            {
                //        mediaElementNews.Clock.IsPaused; mediaElement.SpeedRatio;mediaElement.star
            }
        }

        private void MediaElementNews_MediaEnded(object sender, RoutedEventArgs e)
        {
            //if (mediaElementNews.HasAudio == false)
            //{
            //    mediaElementNews.Pause();
            //    mediaElementNews.Position = mediaElementNews.Position - new TimeSpan(0, 0, 0, 1);
            //    // mediaElement.Position = TimeSpan.FromSeconds(0);
            //    timerDurationShowNews.Start();
            //}
            //else
            //{
            //    Timer_Tick(sender, new EventArgs());
            //}

        }

        /// <summary>
        /// Выполняется при окончании проигрывания ролика
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            //mediaElement.Close();
            //playing = false;
            //if (CheckTime() || StartWithoutTime)
            //{
            //    if (queue > playList.Count)
            //    {
            //        queue = 1;
            //    }
            //    try
            //    {
            //        mediaElement.Source = playList[queue - 1];
            //    }
            //    catch (Exception)
            //    {
            //        mediaElement.Source = playList[0];
            //    }

            //    //mediaElement.Position = TimeSpan.FromSeconds(0);
            //    mediaElement.Play();
            //    queue = GetThreadRandom().Next(1, playList.Count);
            //    // queue++;
            //}
        }


        void CreateNewsWindow(Canvas canvas)//Создание окна новостей
        {
            StackPanel contentControlNews = new StackPanel();

            contentControlNews.Width = canvas.Width;
            contentControlNews.Height = canvas.Height;

            contentControlNews.Background = new SolidColorBrush(Colors.Black);

            mediaElementNews = new MediaElement();
            mediaElementNews.LoadedBehavior = MediaState.Manual;
            mediaElementNews.UnloadedBehavior = MediaState.Manual;
            mediaElementNews.Width = contentControlNews.Width;
            mediaElementNews.Height = contentControlNews.Height;
            mediaElementNews.MediaOpened += MediaElementNews_MediaOpened;
            mediaElementNews.MediaEnded += MediaElementNews_MediaEnded;
            mediaElementNews.KeyDown += MediaElementNews_KeyDown;
            mediaElementNews.Stretch = Stretch.UniformToFill;

            contentControlNews.Children.Add(mediaElementNews);
            contentControlNews.Margin = new Thickness(canvas.Width / 2, canvas.Height, -contentControlNews.Width, 0);
            canvas.Children.Add(contentControlNews);
            canvas.Children[1].Visibility = Visibility.Visible;
            mediaElementNews.Visibility = Visibility.Visible;
            contentControlNews.Visibility = Visibility.Visible;
            contentControlNews.KeyDown += ContentControlNews_KeyDown;
            canvas.KeyDown += Canvas_KeyDown;
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

        public void AnimationNews()//Выезд и уход за экран окна новостей. Проверяет текущее состояние новостей и выполняет требуемые действияю
        {
            Canvas canvas = window.Content as Canvas;
            StackPanel contentControlNews = canvas.Children[3] as StackPanel;           
            ThicknessAnimation thicknessAnimation = new ThicknessAnimation();
            SineEase se = new SineEase();
            se.EasingMode = EasingMode.EaseInOut;
            thicknessAnimation.EasingFunction = se;
            thicknessAnimation.DecelerationRatio = 0.5;

            if (showNews == true)//если истина, то выводим экран новостей
            {
                thicknessAnimation.From = contentControlNews.Margin;
                thicknessAnimation.To = mediaElement.Margin;// new Thickness(canvas.ma, canvas.Height / 2, 0, 0);
            }
            else
            {
                thicknessAnimation.From = contentControlNews.Margin;
                thicknessAnimation.To = new Thickness(mediaElement.Width / 2, mediaElement.Height, 0, 0);
            }

            thicknessAnimation.Duration = TimeSpan.FromSeconds(3);

            contentControlNews.BeginAnimation(StackPanel.MarginProperty, thicknessAnimation);
        }

    }
}
