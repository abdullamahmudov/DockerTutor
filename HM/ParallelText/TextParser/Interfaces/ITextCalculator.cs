using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TextParser.Interfaces
{
    public interface ITextCalculator
    {
        /// <summary>
        /// Посчитать количество пробелов в файле
        /// </summary>
        /// <param name="text">Текст</param>
        /// <returns></returns>
        Task CalculateSpaces(string text);
    }
}