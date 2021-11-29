using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVTK.Entity
{
    public class Dates: IComparable, IComparer
    {
        /// <summary>
        /// Запланированная планировщиком дата
        /// </summary>
        public DateTimeOffset Day { get; set; }
        /// <summary>
        /// Перечень временных отрезков проигрывания контента в течении дня 
        /// </summary>
        public ObservableCollection<TimeOfPlaying> timeOfPlayings { get; set; }
        /// <summary>
        /// Конструктор для копирования времени
        /// </summary>
        /// <param name="_timeOfPlayings"></param>
        public Dates(ObservableCollection<TimeOfPlaying> _timeOfPlayings) 
        {
            timeOfPlayings = _timeOfPlayings;
        }
        /// <summary>
        /// Конструктор с указанием даты и времени
        /// </summary>
        /// <param name="_Day"></param>
        /// <param name="_timeOfPlayings"></param>
        public Dates(DateTimeOffset _Day, ObservableCollection<TimeOfPlaying> _timeOfPlayings):this(_timeOfPlayings)
        {
            Day = _Day;
        }
        /// <summary>
        /// Проверка даты сегодняшнего дня с указанныи
        /// </summary>
        /// <param name="Day">Проверяемый день</param>
        /// <param name="Today">Сегодняшний день</param>
        /// <returns></returns>
        public static bool InToday(DateTimeOffset Day, DateTimeOffset Today) 
        {
            if (Day.Date == Today.Date)
            {
                return true;
            }
            return false;

        }
        /// <summary>
        /// Объединение периодов времени проигрывания конента в разрезе дат.
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <returns></returns>
        public static IEnumerable<Dates> Union(IEnumerable<Dates> d1, IEnumerable<Dates> d2)
        {
            var _temp = Enumerable.Union(d1, d2).ToList();
            var _temp2 = d1.Concat(d2);
            foreach (var d in _temp)
            {
                var i = _temp2.Where(t => t.Day.Date == d.Day.Date);
                if (i != null && i.Count() > 0)
                {
                    ObservableCollection<TimeOfPlaying> _Temp = new ObservableCollection<TimeOfPlaying>();
                    foreach (var t in i)
                    {
                        _Temp = (ObservableCollection<TimeOfPlaying>)TimeOfPlaying.Concat(_Temp, t.timeOfPlayings);
                    }
                    d.timeOfPlayings = _Temp;
                }
            }
            return _temp;
        }
        /// <summary>
        /// Внутренний метод сравнения
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private int CompareTo(Dates obj)
        {
            if (this.Day.Date < (obj as Dates).Day.Date)
            {
                return -1;
            }
            if (this.Day.Date > (obj as Dates).Day.Date)
            {
                return 1;
            }
            return 0;
        }
        /// <summary>
        /// Реализация интерфейса сравнения
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        int IComparer.Compare(object x, object y)
        {
            return (x as Dates).CompareTo(y as Dates);
        }
        /// <summary>
        /// Реализация интерфейса сравнения
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        int IComparable.CompareTo(object obj)
        {
            return ((IComparable)Day).CompareTo(obj);
        }
    }
}
