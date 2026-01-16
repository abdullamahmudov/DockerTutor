using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsoleApp.Interfaces;

namespace ConsoleApp.Implementations
{
    /// <summary>
    /// Генерирует числа с случайным диапозоном
    /// </summary>
    public class RandomGenerator : IGenerator
    {
        /// <inheritdoc/>
        public List<int> GetNumberList(int length)
        {
            var random = new Random();
            var list = new List<int>();
            for (int i = 0; i < length; i++)
            {
                list.Add(random.Next(0, 1000));
            }
            return list;
        }
    }
}