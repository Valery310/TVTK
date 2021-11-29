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
        /// Список всех доступных новостного контента
        /// </summary>

        public Dictionary<uint, uint> List { get; private set; }
        /// <summary>
        /// Порядок воспроизведения новостей, зависимый от ключа TypePlaying.
        /// </summary>

        public uint CurrentIndexMediaFile { get; private set; }
        /// <summary>
        /// Индекс текущего проигрываемого новостного файла
        /// </summary>
        public TypeContent typeContent { get; set; }

    }
}
