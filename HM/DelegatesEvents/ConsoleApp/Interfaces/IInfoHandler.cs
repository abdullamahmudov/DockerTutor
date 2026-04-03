using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsoleApp.Implementations;

namespace ConsoleApp.Interfaces
{
    /// <summary>
    /// Интерфейс для обработки информационных сообщений или данных.
    /// </summary>
    public interface IInfoHandler
    {
        /// <summary>
        /// Обрабатывает переданную информацию.
        /// </summary>
        /// <param name="info">Строка с информацией для обработки.</param>
        void Handle(string info);
    }
}