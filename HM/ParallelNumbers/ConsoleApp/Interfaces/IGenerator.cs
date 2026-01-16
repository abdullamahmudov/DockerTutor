using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp.Interfaces
{
    public interface IGenerator
    {
        /// <summary>
        /// Получить сгенерированный список чисел
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        List<int> GetNumberList(int length);
    }
}