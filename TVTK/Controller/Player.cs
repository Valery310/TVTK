using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TVTK.Entity;
using TVTK.Enums;
using TVTK.Playlist;

namespace TVTK.Controller
{
    public static class Player
    {
        /// <summary>
        /// Длительность показа новостного контента плейлиста. Например: Показывать в течении 10 минут.
        /// </summary>
        public static int DurationShow { get; set; }
        /// <summary>
        /// Показывать новостной контент один раз в указанный период. Например: каждые 20 минут.
        /// </summary>
        public static int FrequencyShow { get; set; }      
        /// <summary>
        /// Порядок проигрывания файлов в плелистах.
        /// </summary>
        public static TypePlaying typePlaying { get; private set; }

        public static PlayList CurrentPlaylist { get; set; }

        //   public static Scheduler scheduler { get; set; }

        static Player() 
        {
            Scheduler.StartPlaying += StartPlayer;
            Scheduler.StopPlaying += StopPlayer;
        }

        /// <summary>
        /// Определяет текущий плелист на основе их приоритетов
        /// </summary>
        public static void SelectPlaylist()
        {

        }

        public static void StopPlayer() //Создание окна проигрывателя и его запуск.
        {
            CurrentPlaylist = null;
        }
            public static void StartPlayer(List<PlayList> playlist) //Создание окна проигрывателя и его запуск.
        {
            PlayList temp = null;
            foreach (var item in playlist)
            {
                if (temp == null) 
                {
                    temp = item;
                    continue;
                }

                if (temp.Priotity < item.Priotity)
                {
                    CurrentPlaylist = item;
                }
            }
            
            CurrentPlaylist = temp;
           


            queue = 1;
            queueNews = 1;

            if (Properties.Settings.Default.News)
            {
                timerStartNews.Tick += new EventHandler(StartNews);
                timerStartNews.Start();
                timer.Start();
            }

            CreatePlayer();

            if (playList != null)
            {
                mediaElement.Source = playList[GetThreadRandom().Next(0, playList.Count)];
                // queue++;
                if (StartWithoutTime)
                {
                    mediaElement.Play();
                }
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
                if (mediaElementNews.Source == null)
                {
                    mediaElementNews.Source = playListNews[GetThreadRandom().Next(0, playListNews.Count)];
                }
                else if (mediaElementNews.HasAudio == false)
                {
                    mediaElementNews.Position = mediaElementNews.Position - new TimeSpan(0, 0, 0, 1);
                }
                showNews = true;
                AnimationNews();
                mediaElementNews.Focus(); //закомментить
                //if (mediaElementNews.HasAudio == false)
                //{
                //    Timer_Tick(mediaElementNews, new EventArgs());
                //}

                mediaElementNews.Play();

            }
        }

        public void StopNews(object sender, EventArgs e)
        {
            if (showNews)
            {
                timerStartNews.Start();
                timerEndNews.Tick -= new EventHandler(StopNews);
                timerEndNews.Stop();
                mediaElementNews.Pause();
                showNews = false;
                AnimationNews();
                mediaElement.Focus();//закомментить
                mediaElement.Position = mediaElement.Position - new TimeSpan(0, 0, 0, 1);
                mediaElement.Play();
                if (mediaElement.HasAudio == false)
                {
                    mediaElement_MediaEnded(mediaElement, new RoutedEventArgs());
                }
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
            screenSaver = new ScreenSaver(canvas);
            screenSaver1 = new ScreenSaver(canvas);
            window.KeyDown += window_KeyDown;
            window.Cursor = Cursors.None;
            if (Properties.Settings.Default.News)
            {
                CreateNewsWindow(canvas);
            }
            //var scaleX = VisualTreeHelper.GetDpi(this).DpiScaleX;
            //var scaleY = VisualTreeHelper.GetDpi(this).DpiScaleY;
            // ((UIElement)Content)
            //window.RenderTransform = new ScaleTransform(1 / scaleX, 1 / scaleY);
            //((UIElement)window.Content).RenderTransform = new ScaleTransform(1 / scaleX, 1 / scaleY);
            //  ((UIElement)window.Content).RenderTransform = new ScaleTransform(1, 1);
            //    ((UIElement)window.Content).RenderTransform = new ScaleTransform(0.989, 0.989);
            window.Show();
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
            // mediaElement.Stretch = Stretch.Uniform;
            mediaElement.Stretch = Stretch.UniformToFill;
            mediaElement.LoadedBehavior = MediaState.Manual;
            mediaElement.UnloadedBehavior = MediaState.Manual;
        }


        /// <summary>
        /// Выполняется при открытии файла плеером.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            playing = true;
        }

        /// <summary>
        /// Выполняется при окончании проигрывания ролика
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            mediaElement.Close();
            playing = false;
            if (CheckTime() || StartWithoutTime)
            {
                if (queue > playList.Count)
                {
                    queue = 1;
                }
                try
                {
                    mediaElement.Source = playList[queue - 1];
                }
                catch (Exception)
                {
                    mediaElement.Source = playList[0];
                }

                //mediaElement.Position = TimeSpan.FromSeconds(0);
                mediaElement.Play();
                queue = GetThreadRandom().Next(1, playList.Count);
                // queue++;
            }
        }


        private void MediaElementNews_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (mediaElementNews.HasAudio == false)
            {
                mediaElementNews.Pause();
                mediaElementNews.Position = mediaElementNews.Position - new TimeSpan(0, 0, 0, 1);
                // mediaElement.Position = TimeSpan.FromSeconds(0);
                timerDurationShowNews.Start();
            }
            else
            {
                Timer_Tick(sender, new EventArgs());
            }

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
                try
                {
                    mediaElementNews.Source = playListNews[queueNews - 1];
                }
                catch (Exception)
                {
                    mediaElementNews.Source = playListNews[0];
                }

                //if (mediaElementNews.HasAudio == false)
                //{
                //  //  mediaElementNews.Position = mediaElementNews.Position - new TimeSpan(0,0,0,1);
                //}
                //  mediaElement.Position = TimeSpan.FromSeconds(0);
                mediaElementNews.Play();
                //queueNews = random.Next(1, playListNews.Count);
                (sender as DispatcherTimer)?.Stop();
                queueNews++;
            }

        }

    }
}
