﻿using System;
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
using Newtonsoft.Json.Serialization;
using NLog;
using TVTK.Entity;
using TVTK.Controller;
using TVTK.Playlist;
using TVTK.Enums;
using NLog.Config;
//using NAudio.CoreAudioApi;

namespace TVTK
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        public static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

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
        List<List<MediaFile>> settingFromServer;
        ScreenSaver screenSaver;
        ScreenSaver screenSaver1;
       // static ThreadLocal<Random> random;
        static Random random;


        public ObservableCollection<TimeOfPlaying> viewModelTime //Коллекция времени работы плеера
        {
            get;
            set;
        }

        public ObservableCollection<TV> viewModelTV //Коллекция телевизоров
        {
            get;
            set;
        }

        public static Random GetThreadRandom()
        {
            return random;
           // return random.Value;
        }

        public MainWindow()
        {
            LogManager.Configuration = new XmlLoggingConfiguration("NLog.config");

            Logger.Info("Старт инициализации компонентонв программы");
  
            InitializeComponent();

            int seek = Environment.TickCount;
            random = new Random(seek);
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
                Properties.Settings.Default.Time = new List<TimeOfPlaying>();
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

            viewModelTime = new ObservableCollection<TimeOfPlaying>(Properties.Settings.Default.Time);

            viewModelTime.CollectionChanged += ViewModelTime_CollectionChanged;

            if (viewModelTime.Count == 0)//если в параметре времени работы нет параметров,то надо их инициализировать дефолтными значениями
            {
                viewModelTime.Add(new TimeOfPlaying { From = DateTimeOffset.Parse("08:00"), Before = DateTimeOffset.Parse("09:00") });
                viewModelTime.Add(new TimeOfPlaying { From = DateTimeOffset.Parse("11:30"), Before = DateTimeOffset.Parse("13:00") });
                viewModelTime.Add(new TimeOfPlaying { From = DateTimeOffset.Parse("16:00"), Before = DateTimeOffset.Parse("17:00") });
            }

            chbScreenSaver.IsChecked = Properties.Settings.Default.ScreenSaver;
            tbxDurationScreenSaver.Text = Properties.Settings.Default.DurationScreenSaver.ToString();

         //   AudioOutput.GetAudio();

            dgTime.ItemsSource = viewModelTime;
            dgTV.ItemsSource = viewModelTV;

            if (Properties.Settings.Default.Autoplay == true)
            {
                ButtonStart_Click(new object(), new RoutedEventArgs());
                this.WindowState = WindowState.Minimized;
            }
        }

        private void ViewModelTV_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Properties.Settings.Default.TVs = viewModelTV.ToList<TV>();
            Properties.Settings.Default.Save();
        }

        private void ViewModelTime_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            
            Properties.Settings.Default.Time = viewModelTime.ToList<TimeOfPlaying>();
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
            CheckPlayer(new object(), new EventArgs());
        }

        public void CheckPlayer(object sender, EventArgs e) //Запуск плеера если наступило рабочее время.
        {
            if (CheckTime() == true && playing == false && StartWithoutTime == false && !showNews)
            {
                //UpdatePlayList();
                TV.WOL(viewModelTV);
                mediaElement.Play();
            }
            else if (CheckTime() == false && playing == false && StartWithoutTime == false && !showNews && chbScreenSaver.IsChecked == true) 
            {
                if (mediaElement.HasAudio == false && mediaElementNews.HasAudio == false)
                {
                    mediaElement.Close();
                    mediaElementNews.Close();
                }
                if (DateTime.Now.Minute % Properties.Settings.Default.DurationScreenSaver == 0)
                {
                    screenSaver?.StartScreenSaver(1);
                    screenSaver1?.StartScreenSaver(2);
                }     
            }
        }
       
        private void MediaElementNews_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (mediaElementNews.HasAudio == false)
            {
        //        mediaElementNews.Clock.IsPaused; mediaElement.SpeedRatio;mediaElement.star
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

        private void btnAddTime_Click(object sender, RoutedEventArgs e)//Добавление времени работы
        {
            var time = new TimeOfPlaying();
            DateTimeOffset TempTime;
            DateTimeOffset.TryParse(tbxBreakBefore.Text,out TempTime);
            time.Before = TempTime;
            DateTimeOffset.TryParse(tbxBreakBefore.Text, out TempTime);
            time.From = TempTime; 
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

            contentControlNews.Background = new SolidColorBrush(Colors.Black);

            mediaElementNews = new MediaElement();
            mediaElementNews.LoadedBehavior = MediaState.Manual;
            mediaElementNews.UnloadedBehavior = MediaState.Manual;
            mediaElementNews.Width = contentControlNews.Width;
            mediaElementNews.Height = contentControlNews.Height;
            mediaElementNews.MediaOpened += MediaElementNews_MediaOpened;
            mediaElementNews.MediaEnded += MediaElementNews_MediaEnded; ;
            mediaElementNews.KeyDown += MediaElementNews_KeyDown;
            mediaElementNews.Stretch = Stretch.UniformToFill;
            
            contentControlNews.Children.Add(mediaElementNews);
            contentControlNews.Margin = new Thickness(canvas.Width/2, canvas.Height, -contentControlNews.Width, 0);
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
                thicknessAnimation.To = new Thickness(mediaElement.Width/2, mediaElement.Height, 0, 0);
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

            uint temp = 0;
            if (chbScreenSaver.IsChecked == true && uint.TryParse(tbxDurationScreenSaver.Text, out temp))
            {
                Properties.Settings.Default.ScreenSaver = true;
                Properties.Settings.Default.DurationScreenSaver = temp;
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
            //e.Handled = !(Char.IsDigit(e.Text, 0));
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var t = dgTV.SelectedItems;
            ObservableCollection<TV> ot = new ObservableCollection<TV>();
            foreach (var item in t)
            {
                ot.Add(item as TV);
            }
            TV.WOL(ot);
        }
    }
}
