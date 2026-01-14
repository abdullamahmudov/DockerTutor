using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParallelFileReader.Interfaces
{
    public interface IReader
    {
        /// <summary>
        /// Прочитать файлы из директории
        /// </summary>
        /// <param name="pathToDirectory">Путь к директории с файлами</param>
        /// <param name="limitFiles"></param>
        /// <returns></returns>
        Task<string[]> ReadFilesFromDirectory(string pathToDirectory, int limitFiles);
    }
}