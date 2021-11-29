using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using TVTK.Entity;
using TVTK.Playlist;

namespace TVTK.Controller
{
    /// <summary>
    /// Задача этого класса проверять текущее время и время начала и конца проигрывания контента и вызвать событие старта показа в классе Player
    /// </summary>
    public static class Scheduler
    {   
        /// <summary>
        /// Список всех плейлистов
        /// </summary>
        static ObservableCollection<PlayList> playLists { get; set; }
        /// <summary>
        /// Таймер проверки времени на старт и остановку проигрывания контента
        /// </summary>
        static DispatcherTimer CheckTimer { get; set; }
        /// <summary>
        /// Таймер планировщика на создание задания на слледующий день
        /// </summary>
        static DispatcherTimer UpdateSchedulerTimer { get; set; }

        static NLog.Logger Logger {get;set;}

        public delegate void Start(List<PlayList> playLists);
        /// <summary>
        /// Событие старта плейлиста
        /// </summary>
        /// <param name="playLists"></param>
        static public event Start StartPlaying;

        public delegate void Stop();
        /// <summary>
        /// Событие остановки плейлиста
        /// </summary>
        static public event Stop StopPlaying;

        /// <summary>
        /// Конструктор планировщика
        /// </summary>
        static Scheduler() 
        {
            Logger = MainWindow.Logger;

            Logger.Info("Инициализация планировщика задач");

            CheckTimer = new DispatcherTimer();
            CheckTimer.Interval = new TimeSpan(0, 1, 0);
            CheckTimer.Tick += CheckTime;
            CheckTimer.Start();

            UpdateScheduler(null, null);

            UpdateSchedulerTimer = new DispatcherTimer();
            UpdateSchedulerTimer.Interval = new TimeSpan(1, 0, 0);
            UpdateSchedulerTimer.Tick += UpdateScheduler;
            UpdateSchedulerTimer.Start();
        }

        /// <summary>
        /// Метод проверяет текущую дату и время и вызывает событие старта и остановки проигрывания контента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void CheckTime(object sender, EventArgs e) 
        {
            Logger.Trace("Проверка времени");

            DateTimeOffset dateTimeOffset = DateTimeOffset.Now;

            List<Playlist.PlayList> _Playlist = null;

            foreach (var playlist in playLists)
            {
                if (playlist.Days.Any(d => Dates.InToday(d.Day, dateTimeOffset)))
                {
                    Dates temp = playlist.Days.First(d => d.Day.Date == dateTimeOffset.Date);

                    if (temp.timeOfPlayings.Any(d => TimeOfPlaying.InPeriod(d, dateTimeOffset))) 
                    {
                        _Playlist.Add(playlist);
                        Logger.Trace("Плейлист добавлен в очередь воспроизведения");
                    }
                }
            }
            if (_Playlist != null && _Playlist.Count > 0)
            {
                Logger.Info("Плановый старт воспроизведения контента");
                StartPlaying(_Playlist);
            }
            else
            {
                Logger.Info("Плановая остановка воспроизведения контента");
                StopPlaying();
            }

        }

        /// <summary>
        /// Каждый день выполняет чистку старых заданий и добавление новых заданий на следующие два дня
        /// </summary>
        private static void UpdateScheduler(object sender, EventArgs e) 
        {
            Logger.Info("Плановое обновление задач планировщика");

            DateTimeOffset PreviewDay = DateTimeOffset.Now - TimeSpan.FromDays(1) ;
            DateTimeOffset ToDay = DateTimeOffset.Now;
            DateTimeOffset NextDay = DateTimeOffset.Now + TimeSpan.FromDays(1);

            ObservableCollection<Dates> Days = new ObservableCollection<Dates>();

            Dates day;

            if (playLists != null)
            {
                foreach (var playlist in playLists)
                {
                    day = playlist.Days.FirstOrDefault();

                    switch (playlist.dayOfWeek)
                    {
                        case Enums.DayOfWeek.Monday:
                            if (NextDay.DayOfWeek == DayOfWeek.Monday)
                            {
                                playlist.Days.Add(new Dates(DateTimeOffset.Now.AddDays(1), day.timeOfPlayings));
                            }
                            break;
                        case Enums.DayOfWeek.Tuesday:
                            if (NextDay.DayOfWeek == DayOfWeek.Monday)
                            {
                                playlist.Days.Add(new Dates(DateTimeOffset.Now.AddDays(1), day.timeOfPlayings));
                            }
                            break;
                        case Enums.DayOfWeek.Wednesday:
                            if (NextDay.DayOfWeek == DayOfWeek.Monday)
                            {
                                playlist.Days.Add(new Dates(DateTimeOffset.Now.AddDays(1), day.timeOfPlayings));
                            }
                            break;
                        case Enums.DayOfWeek.Thursday:
                            if (NextDay.DayOfWeek == DayOfWeek.Monday)
                            {
                                playlist.Days.Add(new Dates(DateTimeOffset.Now.AddDays(1), day.timeOfPlayings));
                            }
                            break;
                        case Enums.DayOfWeek.Friday:
                            if (NextDay.DayOfWeek == DayOfWeek.Monday)
                            {
                                playlist.Days.Add(new Dates(DateTimeOffset.Now.AddDays(1), day.timeOfPlayings));
                            }
                            break;
                        case Enums.DayOfWeek.Saturday:
                            if (NextDay.DayOfWeek == DayOfWeek.Monday)
                            {
                                playlist.Days.Add(new Dates(DateTimeOffset.Now.AddDays(1), day.timeOfPlayings));
                            }
                            break;
                        case Enums.DayOfWeek.Sunday:
                            if (NextDay.DayOfWeek == DayOfWeek.Monday)
                            {
                                playlist.Days.Add(new Dates(DateTimeOffset.Now.AddDays(1), day.timeOfPlayings));
                            }
                            break;
                        case Enums.DayOfWeek.All:                   
                            if (day != null)
                            {
                                playlist.Days.Add(new Dates(DateTimeOffset.Now.AddDays(1), day.timeOfPlayings));
                            }
                            break;
                        case Enums.DayOfWeek.Weekdays:
                            if (NextDay.DayOfWeek == DayOfWeek.Monday || NextDay.DayOfWeek == DayOfWeek.Tuesday 
                                || NextDay.DayOfWeek == DayOfWeek.Wednesday || NextDay.DayOfWeek == DayOfWeek.Thursday 
                                || NextDay.DayOfWeek == DayOfWeek.Friday)
                            {
                                playlist.Days.Add(new Dates(DateTimeOffset.Now.AddDays(1), day.timeOfPlayings));
                            }
                            break;
                        case Enums.DayOfWeek.Weekends:
                            if (NextDay.DayOfWeek == DayOfWeek.Saturday || NextDay.DayOfWeek == DayOfWeek.Sunday)
                            {
                                playlist.Days.Add(new Dates(DateTimeOffset.Now.AddDays(1), day.timeOfPlayings));
                            }
                            break;
                        default:
                            Logger.Warn("Невозможно запланировать проигрывание контента, так как способ планирования для плейлиста не задан.");
                            break;
                    }
                    Logger.Info("Планирование завершено");
                }
                foreach (var d in Days.Where(p => p.Day.Date < ToDay))
                {
                    Days.Remove(d);
                }

                Logger.Info("Устарешие задачи удалены");
            }  
        }
    }
}
