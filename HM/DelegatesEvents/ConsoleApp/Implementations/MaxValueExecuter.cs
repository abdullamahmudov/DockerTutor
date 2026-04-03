using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsoleApp.Interfaces;
using ConsoleApp.Extensions;

namespace ConsoleApp.Implementations
{
    /// <summary>
    /// Исполнитель для поиска максимального элемента в объекте из файла.
    /// </summary>
    /// <typeparam name="TObject">Тип объекта, содержащего коллекцию.</typeparam>
    /// <typeparam name="TElement">Тип элементов в коллекции.</typeparam>
    public class MaxValueExecuter<TObject, TElement> : IInfoHandler
    where TObject : class
    where TElement : class
    {
        private readonly Logger _logger;
        private readonly FileReaderWriter _readerWriter;
        private readonly Func<TObject, IEnumerable> _collectionExecuter;
        private readonly Func<TElement, float> _elementToValue;

        /// <summary>
        /// Инициализирует новый экземпляр MaxValueExecuter.
        /// </summary>
        /// <param name="logger">Компонент для записи логов.</param>
        /// <param name="readerWriter">Компонент для чтения/записи файлов.</param>
        /// <param name="collectionExecuter">Функция для извлечения коллекции из объекта.</param>
        /// <param name="elementToValue">Функция для преобразования элемента в числовое значение.</param>
        public MaxValueExecuter(
            Logger logger,
            FileReaderWriter readerWriter,
            Func<TObject, IEnumerable> collectionExecuter,
            Func<TElement, float> elementToValue)
        {
            _logger = logger;
            _readerWriter = readerWriter;
            _collectionExecuter = collectionExecuter;
            _elementToValue = elementToValue;
        }

        /// <summary>
        /// Обрабатывает файл, находит максимальный элемент и логирует результат.
        /// </summary>
        /// <param name="info">Путь к файлу для обработки.</param>
        public void Handle(string info)
        {
            var file = _readerWriter.ReadFile(info);
            if(file is null)
            {
                _logger.Write(string.Concat("\nНе удалось прочитать файла (", Path.GetFileName(info), ")!"));
                return;
            }
            var obj = System.Text.Json.JsonSerializer.Deserialize<TObject>(file);
            if (obj is null)
            {
                _logger.Write(string.Concat("\nНе удалось извлечь объект из файла (", Path.GetFileName(info), ")!"));
                return;
            }
            var enumerable = _collectionExecuter.Invoke(obj);
            if (enumerable is null)
            {
                _logger.Write(string.Concat("\nНе удалось извлечь коллекцию из объекта в файла (", Path.GetFileName(info), ")!"));
                return;
            }
            var maxElement = enumerable.GetMax(_elementToValue);
            if (maxElement is null)
            {
                _logger.Write(string.Concat("\nНе удалось извлечь максимальный элемент из файла (", Path.GetFileName(info), ")!"));
                return;
            }
            var elementText = System.Text.Json.JsonSerializer.Serialize(maxElement);
            _logger.Write(string.Concat("\nМаксимальный элемент в файле (", Path.GetFileName(info), ") извлечен: \n", elementText));
        }
    }
}