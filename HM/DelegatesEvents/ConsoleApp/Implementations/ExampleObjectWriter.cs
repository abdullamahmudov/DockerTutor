using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsoleApp.Models;

namespace ConsoleApp.Implementations
{
    /// <summary>
    /// Класс для фоновой записи сгенерированных объектов в файлы.
    /// </summary>
    public class ExampleObjectWriter : AFonTask
    {
        /// <summary>
        /// Инициализирует новый экземпляр ExampleObjectWriter.
        /// </summary>
        /// <param name="readerWriter">Компонент для чтения/записи файлов.</param>
        /// <param name="delay">Задержка между записями в миллисекундах.</param>
        public ExampleObjectWriter(FileReaderWriter readerWriter, int delay) : base(readerWriter, delay)
        { }

        /// <summary>
        /// Выполняет процесс генерации и записи объекта в файл.
        /// </summary>
        protected override void Process()
        {
            var fileName = string.Concat(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), ".json");
            var obj = ExampleClass.Generate();
            var objText = System.Text.Json.JsonSerializer.Serialize(obj);
            _readerWriter.WriteFile(_readerWriter.PathToFiles, fileName, objText);
        }
    }
}