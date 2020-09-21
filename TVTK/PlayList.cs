using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVTK
{
    public class PlayList
    {
        public Dictionary<int, MultimediaFile> MediaFiles;//порядок воспроизведения зависит от ключа.
        public int Duration { get; set; }//длительность показа контента плейлиста
        public int Frequency { get; set; }//показывать раз в столько то минут.
        public Dictionary<DayOfWeek, Time> Days = new Dictionary<DayOfWeek, Time>();// в разные дни недели может быть разное время проигрывания плейлиста
        public TypeContent TypeContent { get; set; } //может пригодится во время обработок плелйиста. Пока не придумал =)

        public Dictionary<int, MultimediaFile> FindMultimediaFromPath(string Path)
        {
            return new Dictionary<int, MultimediaFile>();
        }
    }

  

    public enum TypeContent
    {
        Adv,
        News,
        Photo,
        Statistics
    }

    public enum DayOfWeek {Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday}
}
