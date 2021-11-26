using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using EasyWakeOnLan;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TVTK.Controller
{
    public class TV : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string IP { get; set; }
        public string Mac { get; set; }
        private string status;
        public string Status {
            get { return status; }
            set
            {
                status = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public TV() { Status = "-"; }//без конструктора по-умолчанию не сохраняет список в параметрах

        public TV(string name, string ip, string mac, string description = "")
        {
            Name = name;
            IP = ip;
            Mac = mac;
            Description = description;
            try
            {
                string msgerr = "";
                if (!string.IsNullOrWhiteSpace(name))
                {
                    Name = name;
                }
                else
                {
                    msgerr += "Имя ТВ - обязательный атрибут!\n";
                    throw new Exception(msgerr);
                }
                Description = description;
                IPAddress tempIP;
                if (IPAddress.TryParse(ip, out tempIP))
                {
                    IP = ip;
                }
                else
                {
                    msgerr += "IP - обязательный атрибут!\n";
                    throw new Exception(msgerr);
                }
                if (!string.IsNullOrWhiteSpace(mac))
                {
                    Mac = mac;
                }
                else
                {
                    msgerr += "MAC - обязательный атрибут!\n";
                    throw new Exception(msgerr);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public PhysicalAddress GetPhysicalAddress()
        {
            return PhysicalAddress.Parse(Mac);
        }

        public void SetMAC(string mac)
        {
            if (!string.IsNullOrWhiteSpace(mac))
            {
                Mac = mac;
            }
            else
            {
                Exception ex = new Exception("Введите mac правильно.");
                throw ex;
            }
        }

        public IPAddress GetIPAddress()
        {
            return IPAddress.Parse(IP);
        }

        public void SetIP(string ip)
        {
            IPAddress temp;
            if (IPAddress.TryParse(ip, out temp))
            {
                IP = ip;
            }
            else
            {
                Exception ex = new Exception("Введите адрес правильно.");
                throw ex;
            }
        }

        public async static void WOL(ObservableCollection<TV> AllTV)
        {
            foreach (var item in AllTV)
            {
                await Task.Run(()=>{
                    EasyWakeOnLanClient WOLClient = new EasyWakeOnLanClient();
                    for (int i = 0; i < 15; i++)
                    {
                        WOLClient.Wake(item.Mac.ToString());
                    }             
                });
               
            }
        }

        public async static void CheckStatus(ObservableCollection<TV> AllTV) 
        {
            await Task.Run(() =>
            {
                string result = "-";
                int b = 0;
                foreach (var item in AllTV)
                {

                    Ping ping = new Ping();
                    for (int i = 0; i < 5; i++)
                    {
                        PingReply pingresult = ping.Send(item.GetIPAddress(), 500);
                        if (pingresult.Status.ToString() == "Success")
                        {
                            result = "Ok";
                        }
                        else
                        {
                            result = "No";
                        }
                    }

                    item.Status = result;
                    b++;
                }
            });
        
        }
    }
}
