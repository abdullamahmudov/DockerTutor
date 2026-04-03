using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp.Models
{
    /// <summary>
    /// Пример класса, содержащего коллекцию элементов данных.
    /// </summary>
    public class ExampleClass
    {
        /// <summary>
        /// Коллекция элементов данных.
        /// </summary>
        public List<ExampleClassElement> Collection { get; set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса ExampleClass с пустой коллекцией.
        /// </summary>
        public ExampleClass()
        {
            Collection = new List<ExampleClassElement>();
        }

        /// <summary>
        /// Возвращает перечисляемую коллекцию элементов из объекта.
        /// </summary>
        /// <param name="arg">Экземпляр ExampleClass.</param>
        /// <returns>Перечисляемая коллекция элементов.</returns>
        public static IEnumerable ExecuteEnumerable(ExampleClass arg) => arg.Collection;

        /// <summary>
        /// Генерирует новый экземпляр ExampleClass со случайными элементами.
        /// </summary>
        /// <returns>Сгенерированный объект ExampleClass.</returns>
        public static ExampleClass Generate()
        {
            var obj = new ExampleClass();
            var prefix = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var count = Random.Shared.Next(1, 20);
            for (int i = 0; i < count; i++)
            {
                obj.Collection.Add(ExampleClassElement.Generate(string.Concat(prefix, " ", i)));
            }
            return obj;
        }
    }
}