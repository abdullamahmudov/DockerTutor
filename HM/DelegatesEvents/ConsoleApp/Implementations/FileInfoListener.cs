using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsoleApp.Interfaces;
using ConsoleApp.Models;

namespace ConsoleApp.Implementations
{
    /// <summary>
    /// Слушатель событий файлов, передающий информацию обработчику.
    /// </summary>
    public class FileInfoListener
    {
        private readonly IInfoHandler _infoHandler;

        /// <summary>
        /// Инициализирует новый экземпляр FileInfoListener.
        /// </summary>
        /// <param name="inputOutput">Обработчик информации.</param>
        public FileInfoListener(IInfoHandler inputOutput)
        {
            _infoHandler = inputOutput;
        }

        /// <summary>
        /// Подписывается на события монитора директории.
        /// </summary>
        /// <param name="fileChecker">Монитор директории.</param>
        public void Subscribe(IDirectoryMonitor fileChecker)
        {
            fileChecker.SubscribeFileFoundEvent(OutputFileInfoMessage);
        }

        /// <summary>
        /// Отписывается от событий монитора директории.
        /// </summary>
        /// <param name="fileChecker">Монитор директории.</param>
        public void Unsubscribe(IDirectoryMonitor fileChecker)
        {
            fileChecker.UnsubscribeFileFoundEvent(OutputFileInfoMessage);
        }

        /// <summary>
        /// Обрабатывает событие найденного файла и передает путь обработчику.
        /// </summary>
        /// <param name="instance">Источник события.</param>
        /// <param name="arg">Аргументы события с путем файла.</param>
        private void OutputFileInfoMessage(object? instance, FileEventArgs arg)
        {
            _infoHandler.Handle(arg.FilePath);
        }
    }
}