using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TVTK.Controller;
using TVTK.Controller.Properties;
using TVTK.Entity;

namespace TVTK.ViewModel
{
    /// <summary>
    /// Логика взаимодействия для CreatePlaylist.xaml
    /// </summary>
    public partial class CreatePlaylist : Window
    {
        public CreatePlaylist()
        {
            InitializeComponent();
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
            DateTimeOffset.TryParse(tbxBreakBefore.Text, out TempTime);
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
            e.Handled = !((Char.IsLetterOrDigit(e.Text, 0) || e.Text.ToCharArray()[0] == ':') && e.Text == " " && e.Text.Length <= 95);
        }

        private void tbxIPTV_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !((Char.IsDigit(e.Text, 0) || e.Text.ToCharArray()[0] == '.') && e.Text.ToCharArray()[0] != ' ' && e.Text.ToCharArray().Length <= 15);
        }

        private void btnCheckStatus_Click(object sender, RoutedEventArgs e)
        {
            TVWol.CheckStatus(Settings.viewModelTV);
        }
    }
}
