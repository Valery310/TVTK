using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TVTK.Entity;
using TVTK.Enums;

namespace TVTK.Controller
{
    class Player
    {
        /// <summary>
        /// Длительность показа новостного контента плейлиста. Например: Показывать в течении 10 минут.
        /// </summary>
        public int DurationShow { get; set; }
        /// <summary>
        /// Показывать новостной контент один раз в указанный период. Например: каждые 20 минут.
        /// </summary>
        public int FrequencyShow { get; set; }      
        /// <summary>
        /// Порядок проигрывания файлов в плелистах.
        /// </summary>
        public TypePlaying typePlaying { get; private set; }

        public static Dictionary<DateTimeOffset, List<TimeOfPlaying>> TimeOfPlay { get; private set; }// в разные дни недели может быть разное время проигрывания разных плелйистов плейлиста. День недели/Список времени проигрывания в течении дни/id плейлиста
    }
}
