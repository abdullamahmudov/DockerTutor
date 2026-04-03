using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsoleApp.Interfaces;

namespace ConsoleApp.Implementations
{
    /// <summary>
    /// Логгер для событий мониторинга файлов.
    /// </summary>
    public class FileMonitorLogger : IInfoHandler
    {
        private readonly Logger _logger;

        /// <summary>
        /// Инициализирует новый экземпляр FileMonitorLogger.
        /// </summary>
        /// <param name="logger">Компонент для записи логов.</param>
        public FileMonitorLogger(Logger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Обрабатывает информацию о файле, записывая сообщение в лог.
        /// </summary>
        /// <param name="info">Путь к файлу.</param>
        public void Handle(string info)
        {
            var message = string.Concat("\nФайл обнаружен: ", Path.GetFileName(info));
            _logger.Write(message);
        }
    }
}