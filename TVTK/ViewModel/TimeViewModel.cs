using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVTK.ViewModel
{
    public class TimeViewModel: INotifyPropertyChanged
    {
        public TimeViewModel()
        {
        }

        public void RefreshDate()
        {
            NowPlusTen = DateTime.UtcNow.AddMinutes(10);
            RaisePropertyChanged("NowPlusTen");

        }

        public void RaisePropertyChanged(string v)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(v));
            }
        }

        public DateTime NowPlusTen { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
