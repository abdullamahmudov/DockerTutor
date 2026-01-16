using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ConsoleApp.Interfaces;
using ConsoleApp.Models;

namespace ConsoleApp.Implementations
{
    public class DefaultTester : ITester
    {
        /// <inheritdoc/>
        public async Task<TestInfo> Testing(List<int> list, List<ICalculator> methods)
        {
            var testInfo = new TestInfo(list.Count);

            foreach (var method in methods)
            {
                var newList = new List<int>(list);
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var result = await method.GetSum(newList);
                stopwatch.Stop();
                testInfo.MethodInfos.Add(new TestMethodInfo(method.Name, stopwatch.ElapsedMilliseconds, result));
            }
            return testInfo;
        }
    }
}