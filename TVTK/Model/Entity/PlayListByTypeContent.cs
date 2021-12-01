using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TVTK.Enums;
using TVTK.Playlist;

namespace TVTK.Entity
{
    public class PlayListByTypeContent
    {
        /// <summary>
        /// Список доступных файлов рекламного контента
        /// </summary>
        public FileList FileList { get; set; }

        /// <summary>
        /// Плейлист
        /// </summary>
        public Dictionary<uint, uint> List { get; private set; } = new Dictionary<uint, uint>();

        /// <summary>
        /// Индекс текущего файла
        /// </summary>
        private uint CurrentIndexMediaFile { get; set; } = 0;

        /// <summary>
        /// Тип контента
        /// </summary>
        public TypeContent typeContent { get; set; }

        /// <summary>
        /// Тип воспроизведения
        /// </summary>
        public TypePlaying typePlaying { get; set; }

        private static NLog.Logger logger { get; set; }

        /// <summary>
        /// Инициализатор плейлиста на основе списка файлов и указанного типа контента
        /// </summary>
        /// <param name="_FileList"></param>
        /// <param name="_typeContent"></param>
        public PlayListByTypeContent(FileList _FileList, TypeContent _typeContent, TypePlaying _typePlaying)
        {
            logger = MainWindow.Logger;
            logger.Info("Запущена инициализация плейлиста");
            FileList = _FileList;
            typeContent = _typeContent;
            typePlaying = _typePlaying;
            GeneratePlaylist();
        }

        /// <summary>
        /// Получение следующего файла
        /// </summary>
        /// <returns></returns>
        public MediaFile Next() 
        {
            logger.Info("Запрошен следующий файл");
            uint _nextIndex = CurrentIndexMediaFile + 1;
            if (_nextIndex < List.Count)
            {
                CurrentIndexMediaFile = _nextIndex;
                logger.Trace($"Передан файл: {FileList[List[_nextIndex]].NameOfFile}");
                return FileList[List[_nextIndex]];
            }
            else
            if (_nextIndex == List.Count)
            {
                CurrentIndexMediaFile = 0;
                return Next();
            }
            {
                logger.Error("Запрошенный файл не существует, так как его индекс выходит за значение длины списка");
                new ArgumentOutOfRangeException("Запрошенный файл не существует, так как его индекс выходит за значение длины списка");
                return null;
            }
        }

        /// <summary>
        /// Получение предыдущего файла
        /// </summary>
        /// <returns></returns>
        public MediaFile Prev() 
        {
            logger.Info("Запрошен предыдущий файл");
            uint _prevtIndex = CurrentIndexMediaFile - 1;
            if (_prevtIndex < List.Count && FileList != null)
            {
                CurrentIndexMediaFile = _prevtIndex;
                logger.Trace($"Передан файл: {FileList[List[_prevtIndex]].NameOfFile}");
                return FileList[List[_prevtIndex]];
            }
            else
            {
                logger.Error("Запрошенный файл не существует, так как его индекс выходит за значение длины списка");
                new ArgumentOutOfRangeException("Запрошенный файл не существует, так как его индекс выходит за значение длины списка");
                return null;
            }
        }

        /// <summary>
        /// Получение текущего файла
        /// </summary>
        /// <returns></returns>
        public MediaFile Current() 
        {
            logger.Info("Запрошен текущий файл");
            if (FileList != null)
            {
                logger.Trace($"Передан файл: {FileList[List[CurrentIndexMediaFile]].NameOfFile}");
                CurrentIndexMediaFile++;
                return FileList[List[CurrentIndexMediaFile-1]];
            }
            else
            {
                logger.Error("Запрошенный файл не существует, так как его индекс выходит за значение длины списка");
                new ArgumentOutOfRangeException("Запрошенный файл не существует, так как его индекс выходит за значение длины списка");
                return null;
            }


        }

        /// <summary>
        /// Получение следующего файла
        /// </summary>
        /// <returns></returns>
        public uint NextIndex()
        {
            logger.Info("Запрошен индекс следующего файла");
            uint _nextIndex = CurrentIndexMediaFile + 1;

            if (_nextIndex < List.Count)
            {
                CurrentIndexMediaFile = _nextIndex;
                logger.Trace($"Передан индекс: {_nextIndex}");
                return List[_nextIndex];
            }
            else
            {
                logger.Error("Запрошенный индекс не существует, так как его индекс выходит за значение длины списка");
                new ArgumentOutOfRangeException("Запрошенный индекс не существует, так как его индекс выходит за значение длины списка");
                return 0;
            }           
        }

        /// <summary>
        /// Получение предыдущего файла
        /// </summary>
        /// <returns></returns>
        public uint PrevIndex()
        {
            logger.Info("Запрошен индекс предыдущего файла");
            uint _prevtIndex = CurrentIndexMediaFile - 1;
            if (_prevtIndex < List.Count && FileList != null)
            {
                CurrentIndexMediaFile = _prevtIndex;
                logger.Trace($"Передан индекс: {_prevtIndex}");
                return List[_prevtIndex];
            }
            else
            {
                logger.Error("Запрошенный индекс не существует, так как его индекс выходит за значение длины списка");
                new ArgumentOutOfRangeException("Запрошенный индекс не существует, так как его индекс выходит за значение длины списка");
                return 0;
            }
        }

        /// <summary>
        /// Получение текущего файла
        /// </summary>
        /// <returns></returns>
        public uint CurrentIndex()
        {
            logger.Info("Запрошен индекс текущего файла");
            if (FileList != null)
            {
                logger.Trace($"Передан индекс: {CurrentIndexMediaFile}");
                return List[CurrentIndexMediaFile];
            }
            else
            {
                logger.Error("Запрошенный индекс не существует, так как его индекс выходит за значение длины списка");
                new ArgumentOutOfRangeException("Запрошенный индекс не существует, так как его индекс выходит за значение длины списка");
                return 0;
            }
        }

        /// <summary>
        /// Генерация плейлиста
        /// </summary>
        /// <param name="typePlaying"></param>
        private void GeneratePlaylist() 
        {
            logger.Info("Старт генерации плейлиста");
            uint i = 0;

            switch (typePlaying)
            {
                case TypePlaying.Random:              
                    Random random = new Random();
                    foreach (var file in FileList.MediaFiles)
                    {
                        List.Add(i, (uint)random.Next(0, FileList.MediaFiles.Count - 1));
                    }
                    break;
                case TypePlaying.Sequence:
                    foreach (var file in FileList.MediaFiles)
                    {
                        List.Add(i, i);
                    }
                    break;
                case TypePlaying.Loop:
                    List.Add(i, CurrentIndexMediaFile);
                    break;
                default:
                    goto case TypePlaying.Sequence;
            }

            logger.Info("Плейлист сгенерирован");
        }

    }
}
