using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsoleApp.Models;

namespace ConsoleApp.Interfaces
{
    public interface ITester
    {
        Task<TestInfo> Testing(List<int> list, List<ICalculator> methods);
    }
}