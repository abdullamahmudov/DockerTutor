using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp.Interfaces
{
    public interface ICalculator
    {
        /// <summary>
        /// Наименование метода
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Получить сумму чисел
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        Task<int> GetSum(List<int> list);
    }
}