using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsoleApp.Interfaces;

namespace ConsoleApp.Implementations
{
    /// <summary>
    /// Компонент для записи логов в файл.
    /// </summary>
    public class Logger
    {
        private readonly FileReaderWriter _readerWriter;

        /// <summary>
        /// Инициализирует новый экземпляр Logger.
        /// </summary>
        /// <param name="readerWriter">Компонент для чтения/записи файлов.</param>
        public Logger(FileReaderWriter readerWriter)
        {
            _readerWriter = readerWriter;
        }

        /// <summary>
        /// Записывает информационное сообщение в лог-файл.
        /// </summary>
        /// <param name="info">Сообщение для записи.</param>
        public void Write(string info)
        {
            if (string.IsNullOrEmpty(info))
                return;

            var log = _readerWriter.ReadFile(Path.Combine(_readerWriter.PathToLogs, "log.txt"));
            log += info;
            _readerWriter.WriteInFile(_readerWriter.PathToLogs, "log.txt", log);
        }
    }
}