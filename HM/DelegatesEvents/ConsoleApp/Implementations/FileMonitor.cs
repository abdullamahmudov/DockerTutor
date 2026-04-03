using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsoleApp.Interfaces;
using ConsoleApp.Models;

namespace ConsoleApp.Implementations
{
    /// <summary>
    /// Монитор директории файлов, вызывающий события при обнаружении новых файлов.
    /// </summary>
    public class FileMonitor : AFonTask, IDirectoryMonitor
    {
        private event EventHandler<FileEventArgs> FileFound;

        /// <summary>
        /// Вызывает событие FileFound для указанного пути файла.
        /// </summary>
        /// <param name="filePath">Путь к найденному файлу.</param>
        private void InvokeFileFound(string filePath)
        {
            lock (_fileFoundEventLocker)
                FileFound.Invoke(this, new FileEventArgs { FilePath = filePath });
        }
        private object _fileFoundEventLocker = new object();

        /// <summary>
        /// Подписывается на событие обнаружения файла.
        /// </summary>
        /// <param name="method">Обработчик события.</param>
        public void SubscribeFileFoundEvent(EventHandler<FileEventArgs> method)
        {
            lock (_fileFoundEventLocker)
                FileFound += method;
        }

        /// <summary>
        /// Отписывается от события обнаружения файла.
        /// </summary>
        /// <param name="method">Обработчик события для отписки.</param>
        public void UnsubscribeFileFoundEvent(EventHandler<FileEventArgs> method)
        {
            lock (_fileFoundEventLocker)
                FileFound -= method;
        }
        private List<string> _readedFiles = new List<string>();

        /// <summary>
        /// Список уже прочитанных файлов.
        /// </summary>
        private List<string> ReadedFiles
        {
            get
            {
                lock (_readedFilesLocker)
                    return _readedFiles;
            }
        }
        private object _readedFilesLocker = new object();

        /// <summary>
        /// Инициализирует новый экземпляр FileMonitor.
        /// </summary>
        /// <param name="readerWriter">Компонент для чтения/записи файлов.</param>
        /// <param name="delay">Задержка между проверками в миллисекундах.</param>
        public FileMonitor(FileReaderWriter readerWriter, int delay) : base(readerWriter, delay)
        {
            FileFound += (obj, arg) => { };
        }

        /// <summary>
        /// Выполняет процесс мониторинга директории и вызывает события для новых файлов.
        /// </summary>
        protected override void Process()
        {
            var path = _readerWriter.PathToFiles;
            var files = _readerWriter.GetFilesPath(path);
            var readedFiles = ReadedFiles.ToHashSet();
            foreach (var filePath in files)
            {
                if (readedFiles.Contains(filePath))
                    continue;

                ReadedFiles.Add(filePath);
                InvokeFileFound(filePath);
            }
        }
    }
}