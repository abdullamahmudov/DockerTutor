using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp.Models
{
    /// <summary>
    /// Элемент данных с именем и весовыми значениями.
    /// </summary>
    public class ExampleClassElement
    {
        /// <summary>
        /// Имя элемента.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Первый весовой коэффициент.
        /// </summary>
        public float Weight1 { get; set; }

        /// <summary>
        /// Второй весовой коэффициент.
        /// </summary>
        public float Weight2 { get; set; }

        /// <summary>
        /// Вычисляет общий вес элемента.
        /// </summary>
        /// <param name="arg">Элемент для вычисления.</param>
        /// <returns>Сумма весов Weight1 и Weight2.</returns>
        public static float ExecuteWeight(ExampleClassElement arg) => arg.Weight1 + arg.Weight2;

        /// <summary>
        /// Генерирует новый элемент с случайными весами.
        /// </summary>
        /// <param name="name">Имя для элемента.</param>
        /// <returns>Сгенерированный элемент.</returns>
        public static ExampleClassElement Generate(string name) =>
        new ExampleClassElement
        {
            Name = name,
            Weight1 = Random.Shared.NextSingle() * 100,
            Weight2 = Random.Shared.NextSingle() * 100,
        };
    }
}