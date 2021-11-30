using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TVTK.Enums;
using System.IO;

namespace TVTK.Entity
{
    public class MediaFile
    {
        /// <summary>
        /// Путь к файлу
        /// </summary>
        public Uri PathToFile { get; }
        /// <summary>
        /// Имя файла
        /// </summary>
        public String NameOfFile { get; }
        /// <summary>
        /// Расширение файла
        /// </summary>
        public String Extension { get; }
        /// <summary>
        /// Тип медиа
        /// </summary>
        public TypeMediaFile Type { get; }
        /// <summary>
        /// Размер файла
        /// </summary>
        public long Size { get; }
        /// <summary>
        /// Существоввание файла
        /// </summary>
        public bool Exsist { get; set; }

        private NLog.Logger Logger { get; }

        /// <summary>
        /// Создание нового объекта файла
        /// </summary>
        /// <param name="_PathToFile"></param>
        public MediaFile(Uri _PathToFile)
        {
            Logger = MainWindow.Logger;
            if (File.Exists(_PathToFile.ToString()))
            {
                using (FileStream file = File.OpenRead(_PathToFile.ToString()))
                {
                    PathToFile = _PathToFile;
                    Exsist = true;
                    NameOfFile = Path.GetFileNameWithoutExtension(file.Name);
                    Extension = Path.HasExtension(file.Name) is true ? Path.GetExtension(file.Name) : null;
                    Type = SetType(Extension);
                    Size = file.Length;
                }
            }
            else
            {
                Logger.Warn($"Файл по пути {0} не существует", _PathToFile.ToString());
            }
        }

        /// <summary>
        /// Получение типа файла по его расширению
        /// </summary>
        /// <param name="Ext"></param>
        /// <returns></returns>
        private static TypeMediaFile SetType(string Ext)
        {
            foreach (ExtensionsImageMediaFile item in Enum.GetValues(typeof(ExtensionsImageMediaFile)))
            {
                if (Ext == item.ToString())
                {
                    return TypeMediaFile.Image;
                }              
            }
            foreach (ExtensionsAudioMediaFile item in Enum.GetValues(typeof(ExtensionsAudioMediaFile)))
            {
                if (Ext == item.ToString())
                {
                    return TypeMediaFile.Audio;
                }
            }
            foreach (ExtensionsVideoMediaFile item in Enum.GetValues(typeof(ExtensionsVideoMediaFile)))
            {
                if (Ext == item.ToString())
                {
                    return TypeMediaFile.Video;
                }
            }
            return TypeMediaFile.Unknown;
        }

        /// <summary>
        /// Получение имени файла для визуальных компонентов и списков
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return NameOfFile;
        }
    }
}
