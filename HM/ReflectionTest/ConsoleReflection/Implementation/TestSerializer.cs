using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ConsoleReflection.Interfaces;

namespace ConsoleReflection.Implementation
{
    public class TestSerializer
    {
        private readonly ISerializer _serializer;
        private readonly TableBuilder _tableBuilder;
        private readonly Stopwatch _stopwatch;
        public string? Result { get; private set; }
        public string? TestResult { get; private set; }
        public TestSerializer(ISerializer serializer, TableBuilder tableBuilder, Stopwatch stopwatch)
        {
            _serializer = serializer;
            _tableBuilder = tableBuilder;
            _stopwatch = stopwatch;
        }
        /// <summary>
        /// Тестирует производительность сериализации для разного количества итераций.
        /// </summary>
        /// <param name="obj">Объект для сериализации</param>
        /// <param name="list">Список количеств итераций для тестирования</param>
        /// <typeparam name="T">Тип объекта</typeparam>
        public void Test<T>(T obj, List<int> list)
        {
            TestResult = string.Empty;
            Result = _serializer.Serialize(obj);
            var table = new string[list.Count + 1][];
            table[0] = ["№", "Количество итераций", "время"];
            for (int listIndex = 0; listIndex < list.Count; listIndex++)
            {
                _stopwatch.Reset();
                _stopwatch.Start();
                for (int i = 0; i < list[listIndex]; i++)
                {
                    _serializer.Serialize(obj);
                }
                _stopwatch.Stop();
                table[listIndex + 1] = [(listIndex + 1).ToString(), list[listIndex].ToString(), "" + _stopwatch.ElapsedMilliseconds + " ms"];
            }
            TestResult += string.Concat("\n", _tableBuilder.ToString(table, 10, 23, 16));
        }
        /// <summary>
        /// Выводит результаты сериализации и время выполнения в консоль.
        /// </summary>
        public void Write()
        {
            Console.WriteLine(string.Concat("\nSerializer: ", _serializer.ToString(), "\n", "Результат сериализации: \n"));
            _stopwatch.Reset();
            _stopwatch.Start();
            Console.WriteLine(Result);
            _stopwatch.Stop();
            Console.WriteLine(string.Concat("\nВремя вывода результата: ", _stopwatch.ElapsedMilliseconds, " ms"));
            Console.WriteLine(string.Concat("\n\n", TestResult));
        }
    }
}