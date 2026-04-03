using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp.Extensions
{
    /// <summary>
    /// Расширения для работы с коллекциями.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Находит элемент с максимальным значением в коллекции.
        /// </summary>
        /// <typeparam name="T">Тип элементов в коллекции.</typeparam>
        /// <param name="obj">Коллекция для поиска.</param>
        /// <param name="convertToNumber">Функция преобразования элемента в числовое значение.</param>
        /// <returns>Элемент с максимальным значением или null, если коллекция пуста.</returns>
        public static T? GetMax<T>(this IEnumerable obj, Func<T, float> convertToNumber) where T: class
        {
            float? max = null;
            T? currentMaxElement = null;
            foreach (var element in obj)
            {
                if(element is T elementTyped)
                {
                    var value = convertToNumber.Invoke(elementTyped);
                    if(!max.HasValue || value > max.Value)
                    {
                        max = value;
                        currentMaxElement = elementTyped;
                    }
                }
            }
            return currentMaxElement;
        }
    }
}