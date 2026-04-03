using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsoleApp.Implementations;
using ConsoleApp.Models;

namespace ConsoleApp.Interfaces
{
    /// <summary>
    /// Интерфейс для мониторинга директории и уведомления о найденных файлах.
    /// </summary>
    public interface IDirectoryMonitor
    {
        /// <summary>
        /// Подписывается на событие обнаружения файла.
        /// </summary>
        /// <param name="method">Обработчик события с аргументами FileEventArgs.</param>
        void SubscribeFileFoundEvent(EventHandler<FileEventArgs> method);

        /// <summary>
        /// Отписывается от события обнаружения файла.
        /// </summary>
        /// <param name="method">Обработчик события для отписки.</param>
        void UnsubscribeFileFoundEvent(EventHandler<FileEventArgs> method);
    }
}