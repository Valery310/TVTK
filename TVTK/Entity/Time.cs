using System;

namespace TVTK.Entity
{
    public class TimeOfPlaying
    {
       /// <summary>
       /// Время начал трансляции
       /// </summary>
        public DateTimeOffset From { get; set; }
        /// <summary>
        /// Время окончания трансляции
        /// </summary>
        public DateTimeOffset Before { get; set; }
    }
}
