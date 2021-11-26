using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using TVTK.Entity;

namespace TVTK.Controller.Properties {
    /// <summary>
    /// Этот класс позволяет загружать, редактировать и сохранять параметры приложения в файл Settings.settings:
    /// </summary>
    internal sealed partial class Settings {
        
        public Settings() {
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
        private void PropertyChangedEventHandler(object sender, System.Configuration.SettingChangingEventArgs e)
        {
            // Добавьте здесь код для обработки события SettingChangingEvent.
        }
        /// <summary>
        /// Событие SettingsLoaded возникает после загрузки значений параметров.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsLoadedEventHandler(object sender, System.Configuration.SettingChangingEventArgs e)
        {
            // Добавьте здесь код для обработки события SettingChangingEvent.
        }
        /// <summary>
        /// Событие SettingChanging возникает перед изменением значения параметра.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e) {
            // Добавьте здесь код для обработки события SettingChangingEvent.
        }

        /// <summary>
        /// Событие SettingsSaving возникает перед сохранением значений параметров.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e) {
            // Добавьте здесь код для обработки события SettingsSaving.
        }

        public async Task<List<List<MediaFile>>> GetPropertiesFromServer()
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
