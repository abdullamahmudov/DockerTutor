using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ConsoleReflection.Interfaces;

namespace ConsoleReflection.Implementation
{
    public class TestDeserializer<T> where T: class
    {
        private readonly IDeserializer _deserializer;
        private readonly TableBuilder _tableBuilder;
        private readonly Stopwatch _stopwatch;
        public T? Result { get; private set; }
        public string? TestResult { get; private set; }
        public TestDeserializer(IDeserializer serializer, TableBuilder tableBuilder, Stopwatch stopwatch)
        {
            _deserializer = serializer;
            _tableBuilder = tableBuilder;
            _stopwatch = stopwatch;
        }
        /// <summary>
        /// Тестирует производительность десериализации для разного количества итераций.
        /// </summary>
        /// <param name="text">Строка для десериализации</param>
        /// <param name="list">Список количеств итераций для тестирования</param>
        public void Test(string text, List<int> list)
        {
            Result = _deserializer.Deserialize<T>(text);
            var table = new string[list.Count + 1][];
            table[0] = ["№", "Количество итераций", "время"];
            for (int listIndex = 0; listIndex < list.Count; listIndex++)
            {
                _stopwatch.Start();
                for (int i = 0; i < list[listIndex]; i++)
                {
                    _deserializer.Deserialize<T>(text);
                }
                _stopwatch.Stop();
                table[listIndex + 1] = [(listIndex + 1).ToString(), list[listIndex].ToString(), "" + _stopwatch.ElapsedMilliseconds + " ms"];
                _stopwatch.Reset();
            }
            TestResult += string.Concat("\n", _tableBuilder.ToString(table, 10, 23, 16));
        }
        /// <summary>
        /// Выводит результаты десериализации и время выполнения в консоль.
        /// </summary>
        public void Write()
        {
            Console.WriteLine(string.Concat("\nDeserializer: ", _deserializer.ToString(), "\n"));
            Console.WriteLine(string.Concat("\n\n", TestResult));
        }
    }
}