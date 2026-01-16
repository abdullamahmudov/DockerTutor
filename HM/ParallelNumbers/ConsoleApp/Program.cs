// See https://aka.ms/new-console-template for more information
using System.Runtime.CompilerServices;
using ConsoleApp.Implementations;
using ConsoleApp.Interfaces;

Console.WriteLine("Running program!");

var systemInfo = new SystemInfo();
Console.WriteLine(systemInfo.GetInfo());
Console.WriteLine();
Testing(100_000);
Console.WriteLine();
Testing(1_000_000);
Console.WriteLine();
Testing(10_000_000);
Console.WriteLine();

Console.ReadKey();

/// <summary>
/// Запуск тестирования
/// </summary>
/// <param name="length"></param>
void Testing(int length)
{
    IGenerator generator = new RandomGenerator();
    var list = generator.GetNumberList(length);
    ITester tester = new DefaultTester();
    var testInfo = tester.Testing(list, 
        new List<ICalculator> { 
                new SimpleCalculator(),
                new ParallelThreadCalculator(), 
                new ParallelLINQCalculator() })
    .GetAwaiter().GetResult();
    Console.WriteLine(testInfo.ToString(40, 12, 12));
}
