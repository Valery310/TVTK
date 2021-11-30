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
using Newtonsoft.Json.Serialization;
using NLog;
using TVTK.Entity;
using TVTK.Controller;
using TVTK.Playlist;
using TVTK.Enums;
using NLog.Config;
using TVTK.Controller.Properties;
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

        public MainWindow()
        {
            LogManager.Configuration = new XmlLoggingConfiguration("NLog.config");

            Logger.Info("Старт инициализации компонентов программы");
  
            InitializeComponent();

            Player.CreatePlayer();
        }

        internal void ViewModelTV_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
          //  Properties.Settings.Default.TVs = viewModelTV.ToList<TV>();
            Properties.Settings.Default.Save();
        }

        internal void ViewModelTime_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            
           // Properties.Settings.Default.Time = viewModelTime.ToList<TimeOfPlaying>();
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Запуск планировщика с проверкой времени работы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            Scheduler.StartScheduler();
        }

        /// <summary>
        /// Добавление времени работы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddTime_Click(object sender, RoutedEventArgs e)
        {
            var time = new TimeOfPlaying();
            DateTimeOffset TempTime;
            DateTimeOffset.TryParse(tbxBreakBefore.Text,out TempTime);
            time.Before = TempTime;
            DateTimeOffset.TryParse(tbxBreakBefore.Text, out TempTime);
            time.From = TempTime; 
         //   viewModelTime.Add(time);
        }

        /// <summary>
        /// Удаление выделенного времени работы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelTime_Click(object sender, RoutedEventArgs e)
        {
         //   Time t = dgTime.SelectedItem as Time;
          //  viewModelTime.Remove(t);
        }
       
        private void btnApplySetting_Click(object sender, RoutedEventArgs e)
        {
            Settings.Save();
        }
   

        private void rbtypeWork_Checked(object sender, RoutedEventArgs e)
        {
            var rb = sender as RadioButton;
            TypeWork typeWork;

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
                default:
                    goto case TypeWork.Local;
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
            TVWol.WOL(Settings.viewModelTV);
        }

        private void btnAddTV_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var tv = new TVWol(tbxNameTV.Text, tbxIPTV.Text, tbxMACTV.Text, tbxDescTV.Text);
                Settings.viewModelTV.Add(tv);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Введите данные правильно!\n" + ex); ;
            }
        }

        private void btnDelTV_Click(object sender, RoutedEventArgs e)
        {
            TVWol tv = dgTV.SelectedItem as TVWol;
            Settings.viewModelTV.Remove(tv);
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
            TVWol.CheckStatus(Settings.viewModelTV);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var t = dgTV.SelectedItems;
            ObservableCollection<TVWol> ot = new ObservableCollection<TVWol>();
            foreach (var item in t)
            {
                ot.Add(item as TVWol);
            }
            TVWol.WOL(ot);
        }
    }
}
