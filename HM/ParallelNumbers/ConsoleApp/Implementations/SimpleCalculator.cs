using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsoleApp.Interfaces;

namespace ConsoleApp.Implementations
{
    /// <summary>
    /// Обычный способ
    /// </summary>
    public class SimpleCalculator : ICalculator
    {
        /// <inheritdoc/>
        public string Name => "Обычный";
        /// <inheritdoc/>
        public Task<int> GetSum(List<int> list)
        {
            var result = 0;
            foreach (var value in list)
            {
                result += value;
            }
            return Task.FromResult(result);
        }
    }
}