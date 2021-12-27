using LibVLCSharp.Shared;
using LibVLCSharp.WPF;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using TVTK.Controller;
using TVTK.Entity;

namespace TVTK.ViewModel
{
    /// <summary>
    /// Логика взаимодействия для Video.xaml
    /// </summary>
    public partial class Video : Window
    {
        /// <summary>
        /// Таймер времени показа одной новости.
        /// </summary>
        DispatcherTimer timerDurationShowNews;

        /// <summary>
        /// Маркер, позволяющий определить текущее состояние проигрывателя видео/рекламы
        /// </summary>
        bool playing = false;

        /// <summary>
        /// Маркер, похволяющий определить текущее состояние показа новостей. Нужен для таймеров во избежание пвовторного запуска новостей.
        /// </summary>
        public bool showNews = false;

        /// <summary>
        /// Маркер, позволяющий определить тип запуска проигрывания. Варианты: проверять время и соблюдать время работы/ Проигрывать постоянно.
        /// </summary>
        bool StartWithoutTime = false;

        /// <summary>
        /// Окно проигрывателя рекламы/видео
        /// </summary>
        Video window;

        List<List<MediaFile>> settingFromServer;
        ScreenSaver screenSaver;
        ScreenSaver screenSaver1;

        MediaPlayer mediaPlayerNews { get; set; }
        MediaPlayer mediaPlayerAdv { get; set; }

        public static NLog.Logger logger { get; set; }

        public LibVLC libVLC { get; set; }

        private Storyboard storyboard { get; set; }
        //public static Random GetThreadRandom()
        //{
        //    return random;
        //    // return random.Value;
        //}

        public Video()
        {
            LibVLCSharp.Shared.Core.Initialize();
            this.libVLC = new LibVLC();
       
            InitializeComponent();

            logger = MainWindow.Logger;

            window = this;
            this.Title = Properties.Settings.Default.TitlePlayer;
            this.Width = Properties.Settings.Default.Width;
            this.Height = Properties.Settings.Default.Height;

            window.VideoViewCanvas.Width = window.Width;
            window.VideoViewCanvas.Height = window.Height;
          

            screenSaver = new ScreenSaver(VideoViewCanvas);
            screenSaver1 = new ScreenSaver(VideoViewCanvas);
            this.Cursor = Cursors.None;

            this.Show();
            this.Activate();
            window.Left = Properties.Settings.Default.PositionX;
            window.Top = Properties.Settings.Default.PositionY;
            window.KeyDown += Window_KeyDown;

         
   
           

            CreateAdvWindow();

            if (Properties.Settings.Default.News)
            {
                CreateNewsWindow();
                
            }
            // window.WindowStyle = WindowStyle.None;
            //  window.ResizeMode = ResizeMode.NoResize;
            // window.Background = new SolidColorBrush(Colors.Black);


            //  VideoViewAdv.Loaded += VideoViewAdv_Loaded;

            //timerDurationShowNews = new DispatcherTimer();
            //timerDurationShowNews.Interval = new TimeSpan(0, 0, Properties.Settings.Default.DurationShowNews);
            //timerDurationShowNews.Tick += Timer_Tick;

        }

        private void VideoViewAdv_Loaded(object sender, RoutedEventArgs e)
        {
            logger.Info("Плеер реклам загружен");
          //  MessageBox.Show("Плеер реклам загружен");

            mediaPlayerAdv = new MediaPlayer(this.libVLC);
            mediaPlayerAdv.EnableKeyInput = true;
            mediaPlayerAdv.EnableHardwareDecoding = true;
            
            this.VideoViewAdv.Content = mediaPlayerAdv;
            this.VideoViewAdv.MediaPlayer = mediaPlayerAdv;
            window.VideoViewAdv.IsEnabled = true;  
        }

        private void VideoViewNews_Loaded(object sender, RoutedEventArgs e)
        {
            logger.Info("Плеер новостей загружен");
         //     MessageBox.Show("Плеер новостей загружен");

            mediaPlayerNews = new MediaPlayer(this.libVLC);
            mediaPlayerNews.EnableKeyInput = true;
            mediaPlayerNews.EnableHardwareDecoding = true;
            
            mediaPlayerNews.Opening += MediaPlayer_Opening;
            mediaPlayerNews.Stopped += mediaPlayerNews_MediaEnded;

            this.VideoViewNews.Content = mediaPlayerNews;
            this.VideoViewNews.MediaPlayer = mediaPlayerNews;
        }

        private void VideoView_Unloaded(object sender, RoutedEventArgs e)
        {
           // VideoViewNews.MediaPlayer.Stop();
           // VideoViewAdv.MediaPlayer.Stop();
         //   libVLC.Dispose();
          //  VideoViewAdv.MediaPlayer.Dispose();
           // VideoViewNews.MediaPlayer.Dispose();
            
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Start()
        {
            try
            {
                // var media = new Media(libVLC, new Uri("C:\\Users\\Kryukov.vn\\source\\repos\\Valery310\\TVTK\\TVTK\\bin\\Debug\\Test\\gigiena.mp4"));
                //  var media = new Media(libVLC, new Uri("http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4"));
                var media = new Media(libVLC, new Uri("C:\\Users\\valer\\source\\repos\\Valery310\\TVTK\\TVTK\\bin\\Debug\\Test\\dr.jpg"));
                VideoView videoView = null;

                if (showNews)
                {
                    videoView = VideoViewNews;
                }
                else
                {
                    videoView = VideoViewAdv;
                }
                if (videoView.IsLoaded)
                {

                    videoView.MediaPlayer.Play(media);

                    if (showNews)
                    {
                        window.Visibility = Visibility.Visible;
                        AnimationNews();
                    }
                    //this.VideoViewAdv.MediaPlayer.Play(new Media(libVLC, new Uri("http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4")));
                    //  this.mediaPlayerAdv.Play(Player.GetNextMedia(this.libVLC));

                    return true;
                }
                else
                {
                    MessageBox.Show("Плеер еще не загрузился");
                    return false;
                }
              
            }
            catch (Exception)
            {
                return false;
            }
        }

        //public async Task<bool> Start()
        //{
        //    if (await StartAdv() || await StartNews())
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        //public async Task<bool> StartNews()
        //{
        //    try
        //    {
        //        window.Visibility = Visibility.Visible;

        //        // var media = new Media(libVLC, new Uri("C:\\Users\\Kryukov.vn\\source\\repos\\Valery310\\TVTK\\TVTK\\bin\\Debug\\Test\\gigiena.mp4"));
        //        var media = new Media(libVLC, new Uri("C:\\Users\\valer\\source\\repos\\Valery310\\TVTK\\TVTK\\bin\\Debug\\Test\\gigiena.mp4"));
        //        if (window.VideoViewNews.IsLoaded)
        //        {
        //            window.VideoViewNews.MediaPlayer.Play(media);
        //            //this.VideoViewAdv.MediaPlayer.Play(new Media(libVLC, new Uri("http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4")));
        //            //  this.mediaPlayerAdv.Play(Player.GetNextMedia(this.libVLC));
        //            AnimationNews();
        //            return true;
        //        }
        //        else
        //        {
        //            MessageBox.Show("Плеер еще не загрузился");
        //            return false;
        //        }

        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}

        void CreateAdvWindow()
        {

            Thickness thickness = new Thickness(0);

            window.VideoViewAdv.Margin = thickness;


            bool isFullscreen = false;
            bool isPlaying = false;
            Size VideoSize;
            Size FormSize;
            Point VideoLocation;

          //  window.VideoViewAdv.KeyDown += VideoView_KeyDown;
            //this.mediaPlayerAdv = new MediaPlayer(libVLC);
          //  window.VideoViewAdv.Loaded += VideoViewAdv_Loaded;
          //  window.VideoViewAdv.Unloaded += VideoView_Unloaded;
        //   window.VideoViewAdv.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
        //    window.VideoViewAdv.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Blue);
        //    window.VideoViewAdv.BorderThickness = new Thickness(10);

            Size t = new Size(500d, 500d);
            //   VideoViewAdv.RenderSize.Width = t;
            //   VideoViewAdv.RenderSize.Height = t.Height;
            //  this.VideoViewAdv.Style = Style.;
            
           
            this.VideoViewAdv.Height = window.Height;
            this.VideoViewAdv.Width = window.Width;

            //   (sender, e) => VideoViewAdv.MediaPlayer = mediaPlayerAdv;
            //using (var media = new Media(libVLC, new Uri("C:\\Users\\Kryukov.vn\\source\\repos\\Valery310\\TVTK\\TVTK\\bin\\Debug\\Test\\gigiena.mp4")))
            //{
            //    VideoViewAdv.MediaPlayer.Play(media);
            //}
            // var t = Player.GetNextMedia(libVLC);


            // Start(libVLC);
           
        }


        void CreateNewsWindow()//Создание окна новостей
        {
          //  window.VideoViewNews.Loaded += VideoViewNews_Loaded;
         //   VideoViewNews.Unloaded += VideoView_Unloaded;
          
            NewsPanel.Width = VideoViewCanvas.Width;
            NewsPanel.Height = VideoViewCanvas.Height;

            NewsPanel.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Blue);

            // mediaElementNews = new MediaElement();

            // mediaElementNews.LoadedBehavior = MediaState.Manual;
            // mediaElementNews.UnloadedBehavior = MediaState.Manual;
            VideoViewNews.Width = NewsPanel.Width;
            VideoViewNews.Height = NewsPanel.Height;

            //   VideoViewAdv.Loaded += (sender, e) => VideoViewAdv.MediaPlayer = _mediaPlayerAdv;
            //   VideoViewNews.Loaded += (sender, e) => VideoViewNews.MediaPlayer = _mediaPlayerNews;

            //  mediaPlayerNews.Opening.MediaOpened += mediaPlayerNews_MediaOpened;
            //  mediaPlayerNews.MediaEnded += mediaPlayerNews_MediaEnded;
       

         //   VideoViewNews.KeyDown += VideoView_KeyDown;

            // mediaPlayerNews.Stretch = Stretch.UniformToFill;


            NewsPanel.Margin = new Thickness(VideoViewCanvas.Width / 2, VideoViewCanvas.Height, -NewsPanel.Width, 0);

            //VideoViewCanvas.Children[1].Visibility = Visibility.Visible;
            //VideoViewNews.Visibility = Visibility.Visible;
            //NewsPanel.Visibility = Visibility.Visible;

            VideoViewNews.Visibility = Visibility.Collapsed;
            // NewsPanel.KeyDown += ContentControlNews_KeyDown;
            //  VideoViewCanvas.KeyDown += Canvas_KeyDown;

        }

        


            private static void Mp_Stopped(object sender, EventArgs e)
            {
                //     throw new NotImplementedException();
            }

            private static void Mp_MediaChanged(object sender, MediaPlayerMediaChangedEventArgs e)
            {
                //    throw new NotImplementedException();
            }

            private static void _VLCPlayer_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
            {
                //    throw new NotImplementedException();
            }

            private static void Mp_Opening(object sender, EventArgs e)
            {
                //     throw new NotImplementedException();
            }

            private void window_KeyDown(object sender, KeyEventArgs e)//управление проигрыванием плеер и новостей
        {
            switch (e.Key)
            {
                case Key.Pause:
                    if (!showNews)
                    {
                        mediaPlayerAdv.Pause();
                    }
                    else
                    {
                        mediaPlayerNews.Pause();
                    }
                    break;
                case Key.Escape:
                    mediaPlayerNews.Dispose();
                    mediaPlayerAdv.Dispose();
                    TVTK.Controller.Scheduler.StopSheduler();
                    var temp = (sender as Video).Content as Canvas;
                    (temp.Children[0] as VideoView)?.Dispose();
                    (temp.Children[1] as VideoView)?.Dispose();                  
                    (sender as Video).Close();
                    // playList.Clear();
                    //  playListNews.Clear();
                    showNews = false;
                    playing = false;
                    break;
                case Key.Space:
                    if (!showNews)
                    {
                        mediaPlayerAdv.Pause();
                    }
                    else
                    {
                        mediaPlayerNews.Pause();
                    }
                    break;
                case Key.Enter:
                    if (!showNews)
                    {
                        mediaPlayerAdv.Play();
                    }
                    else
                    {
                        mediaPlayerNews.Play();
                    }
                    break;
                case Key.Left:
                    if (!showNews)
                    {
                        mediaPlayerAdv.Position = mediaPlayerAdv.Position - 0.05f;
                    }
                    else
                    {
                        mediaPlayerNews.Position = mediaPlayerNews.Position - 0.05f;
                    }
                    break;
                case Key.Up:
                    if (!showNews)
                    {
                        mediaPlayerAdv.Volume += 5;
                    }
                    else
                    {
                        mediaPlayerNews.Volume += 5;
                    }
                    break;
                case Key.Right:
                    if (!showNews)
                    {
                        var t = mediaPlayerAdv.Position + 0.05f;
                        if (t >= mediaPlayerAdv.Media.Duration)
                        {
                            t = mediaPlayerAdv.Media.Duration;
                        }
                        mediaPlayerAdv.Position = t;
                    }
                    else
                    {
                        var t = mediaPlayerNews.Position + 0.05f;
                        if (t >= mediaPlayerNews.Media.Duration)
                        {
                            t = mediaPlayerNews.Media.Duration;
                        }
                        mediaPlayerNews.Position = t;
                    }
                    break;
                case Key.Down:
                    if (!showNews)
                    {
                        mediaPlayerAdv.Volume -= 5;
                    }
                    else
                    {
                        mediaPlayerNews.Volume -= 5;
                    }
                    break;
                case Key.N:
                    if (!showNews)
                    {
                        mediaPlayerAdv_MediaEnded(mediaPlayerAdv, new EventArgs());
                    }
                    else
                    {
                        mediaPlayerNews_MediaEnded(mediaPlayerNews, new EventArgs());
                    }
                    break;
            }
        }

        private void mediaPlayerNews_MediaEnded(object sender, EventArgs e)
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
        static void mediaPlayerAdv_MediaEnded(object sender, EventArgs e)
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


      

        /// <summary>
        /// Выполняется при открытии файла плеером.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void mediaPlayerNews_MediaOpened(object sender, EventArgs e)
        {
            //    playing = true;
        }

        private void MediaPlayer_Opening(object sender, EventArgs e)
        {
            if (mediaPlayerNews.AudioTrack == -1)
            {
                //        mediaElementNews.Clock.IsPaused; mediaElement.SpeedRatio;mediaElement.star
            }
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

        public async void AnimationNews()//Выезд и уход за экран окна новостей. Проверяет текущее состояние новостей и выполняет требуемые действияю
        {



            //  Canvas canvas = window.Content as Canvas;
            //  StackPanel contentControlNews = canvas.Children[3] as StackPanel;
            ThicknessAnimation thicknessAnimation = new ThicknessAnimation();
            SineEase se = new SineEase();
            se.EasingMode = EasingMode.EaseInOut;
            thicknessAnimation.EasingFunction = se;
            thicknessAnimation.DecelerationRatio = 0.5;

            if (showNews == true)//если истина, то выводим экран новостей
            {
                thicknessAnimation.From = NewsPanel.Margin;
                thicknessAnimation.To = VideoViewAdv.Margin;// new Thickness(canvas.ma, canvas.Height / 2, 0, 0);
            }
            else
            {
                thicknessAnimation.From = NewsPanel.Margin;
                thicknessAnimation.To = new Thickness(VideoViewAdv.Width / 2, VideoViewAdv.Height, 0, 0);
            }

            thicknessAnimation.Duration = TimeSpan.FromSeconds(3);

            storyboard = new Storyboard();
            Storyboard.SetTargetName(thicknessAnimation, NewsPanel.Name);
            Storyboard.SetTargetProperty(thicknessAnimation,
                new PropertyPath(StackPanel.MarginProperty));
            storyboard.Children.Add(thicknessAnimation);
            storyboard.Completed += Storyboard_Completed;

            if (showNews == true)//если истина, то выводим экран новостей
            {
                VideoViewNews.Visibility = Visibility.Visible;
            }

            await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                new Action(() => { 
                    storyboard.Begin(this);                 
                }));

           

            // Dispatcher.Invoke(() => { storyboard.Begin(this); });

            // await Dispatcher.InvokeAsync(() => { storyboard.Begin(this); });

            //  await BeginAnimationAsync(this);
            //NewsPanel.BeginAnimation(StackPanel.MarginProperty, thicknessAnimation);

            //Storyboard storyboard = new Storyboard();
            //storyboard.Be
            //  return this;


        }

        private void Storyboard_Completed(object sender, EventArgs e)
        {
            if (showNews == true)//если истина, то выводим экран новостей
            {
                VideoViewNews.Visibility = Visibility.Collapsed;
            }
        }

        //private static Task BeginAnimationAsync(Video video)
        //{
        //    Storyboard storyboard = video.storyboard;
        //    TaskCompletionSource<object> source = new TaskCompletionSource<object>();
        //    storyboard.Completed += delegate
        //      {
        //          source.SetResult(null);
        //      };
        //    return source.Task;
        //}


        /// <summary>
        /// Бегущая строка с текстовыми новостями
        /// </summary>
        private void CreateNewsRunningLine() 
        {
        
        }

        private void VideoView_KeyDown(object sender, KeyEventArgs e)
        {
            var temp = sender as VideoView;
            if (e.Key == Key.Escape)
            {
                //(sender as Video).Stop();
                libVLC.Dispose();
                //temp.mediaPlayerNews.Dispose();
                //temp.mediaPlayerAdv.Dispose();
                //temp.VideoViewAdv.Dispose();
                //temp.VideoViewNews.Dispose();
               // (sender as VideoView).Close();
            }
        }

        public void Dispose() 
        {
            try
            {
                // videoPlayer.VideoViewNews.MediaPlayer.Stop();
               // window.VideoViewAdv.MediaPlayer.Stop();
                this.mediaPlayerAdv.Stop();
                this.mediaPlayerAdv.Dispose();
                //  videoPlayer.VideoViewNews.MediaPlayer.Dispose();
                this.VideoViewAdv.MediaPlayer.Dispose();
                this.libVLC.Dispose();
            //    window.VideoViewAdv.Dispose();
           //     window.VideoViewNews.Dispose();
                //    videoPlayer.VideoViewAdv.Dispose();
                //  videoPlayer.VideoViewNews.Dispose();
                this.Close();

            }
            catch (Exception ex)
            {
                this.Close();
                logger.Warn($"Ошибка отчистки памяти плеера: {ex.ToString()}");
            }
           
        }

        private void VideoViewCanvas_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void NewsPanel_KeyDown(object sender, KeyEventArgs e)
        {

        }
    }
}
