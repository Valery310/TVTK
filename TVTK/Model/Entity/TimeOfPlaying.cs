using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
        /// <summary>
        /// Проверка попадания времени в указанный период
        /// </summary>
        /// <param name="Period">Указанный период</param>
        /// <param name="dateTimeOffset">Проверяемой время</param>
        /// <returns></returns>
        public static bool InPeriod(TimeOfPlaying Period, DateTimeOffset dateTimeOffset)
        {
            if (TimeSpan.Compare(dateTimeOffset.DateTime.TimeOfDay, Period.From.TimeOfDay) >= 0
                        && TimeSpan.Compare(dateTimeOffset.DateTime.TimeOfDay, Period.Before.TimeOfDay) <= 0)
            {
                return true;
            }
            return false;          
        }
        /// <summary>
        /// Объединение периодов в случае их пересечения на временном отрезке
        /// </summary>
        /// <param name="t2"></param>
        /// <returns></returns>
        public TimeOfPlaying Concat(TimeOfPlaying t)
        {
            if (this.CompareTo(t) == 0)
            { //Четыре ситуации: 1) внутри 2) снаружи) 3) Слева 4) справа
                if (this.From.TimeOfDay >= t.From.TimeOfDay && this.Before.TimeOfDay <= t.Before.TimeOfDay)
                {
                    this.From = t.From;
                    this.Before = t.Before;
                }               
                if (this.From.TimeOfDay < t.From.TimeOfDay && this.Before.TimeOfDay < t.From.TimeOfDay)
                {
                    this.Before = t.Before;
                }
                if (this.From.TimeOfDay < t.From.TimeOfDay && this.Before.TimeOfDay < t.From.TimeOfDay)
                {
                    this.From = t.From;
                }
            }
            return this;
        }
        /// <summary>
        /// Метод объединения пересекающихся временных отрезков
        /// </summary>
        /// <param name="t1"></param>
        /// <returns></returns>
        public static IEnumerable<TimeOfPlaying> Concat(IEnumerable<TimeOfPlaying> t1, IEnumerable<TimeOfPlaying> t2)
        {
            IEnumerable<TimeOfPlaying> timeOfPlayings = new List<TimeOfPlaying>();
            foreach (var i1 in t1)
            {
                foreach (var i2 in t2)
                {
                    if (i1.CompareTo(i2) == 0)
                    {
                        timeOfPlayings.Append(i1.Concat(i2));
                    }
                    else
                    {
                        timeOfPlayings.Append(i1);
                        timeOfPlayings.Append(i2);
                    }

                }         
            }
            return timeOfPlayings;
        }
        /// <summary>
        /// Возвращает -1, если текущий объект идет раньше передаваемого, возвращает 0, если периоды пересекаются, и 1, если текущий период идет позже передаваемого.
        /// </summary>
        /// <param name="t">Сравниваемый объект класса TimeOfPlaying</param>
        /// <returns></returns>
        public int CompareTo(TimeOfPlaying t) 
        {
            if (this.From.TimeOfDay < t.From.TimeOfDay && this.Before.TimeOfDay < t.From.TimeOfDay)
            {
                return -1;
            }
            if (this.From.TimeOfDay > t.Before.TimeOfDay && this.Before.TimeOfDay > t.Before.TimeOfDay)
            {
                return 1;
            }
            return 0;
        }
    }
}
