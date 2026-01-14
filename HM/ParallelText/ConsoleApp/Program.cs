using System.Globalization;
using ParallelFileReader;
using ParallelFileReader.Interfaces;
using TextParser;
using TextParser.Interfaces;

namespace ConsoleApp
{
    public class Program
    {
        public static void Main(params string[] arg)
        {
            if(arg is null || arg.Length != 2)
            {
                throw new ArgumentException();
            }
            if(!int.TryParse(arg[1], out var limitFiles))
            {
                throw new ArgumentException();
            }
            var pathToDirectory = arg[0];
            if(string.IsNullOrWhiteSpace(pathToDirectory))
            {
                throw new ArgumentException();
            }
            CalculateSpacesInFiles(pathToDirectory, limitFiles).GetAwaiter();
        }

        /// <summary>
        /// Подсчитать количество пробелов в файлах по директории
        /// </summary>
        /// <param name="pathToDirectory">Путь к директории с файлами</param>
        /// <param name="limitFiles">Лимит файлов</param>
        /// <returns></returns>
        public static async Task CalculateSpacesInFiles(string pathToDirectory, int limitFiles)
        {
            IReader reader = new Reader();
            ITextCalculator textCalculator = new TextCalculator();
            var texts = reader.ReadFilesFromDirectory(pathToDirectory, limitFiles).GetAwaiter().GetResult();
            var tasks = new List<Task>();
            foreach (var text in texts)
            {
                tasks.Add(Task.Run<Task>(() => textCalculator.CalculateSpaces(text)));
            }
            await Task.WhenAll(tasks);
            Console.WriteLine("\n= = = = = = = = = = \nВСЕ ФАЙЛЫ ПРОЧИТАННЫ И ПОДСЧИТАНЫ!");
        }
    }
}
