using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsoleApp.Interfaces;

namespace ConsoleApp.Implementations
{
    /// <summary>
    /// Параллельный с LINQ
    /// </summary>
    public class ParallelLINQCalculator : ICalculator
    {
        /// <inheritdoc/>
        public string Name => "Параллельный с LINQ";
        
        /// <inheritdoc/>
        public Task<int> GetSum(List<int> list)
        {
            return Task.FromResult(list.AsParallel().Aggregate((x, y) => x + y));
        }
    }
}