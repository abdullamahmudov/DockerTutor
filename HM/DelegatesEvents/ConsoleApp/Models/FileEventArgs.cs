using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp.Models
{
    /// <summary>
    /// Аргументы события для найденного файла.
    /// </summary>
    public class FileEventArgs : EventArgs
    {
        /// <summary>
        /// Путь к найденному файлу.
        /// </summary>
        public required string FilePath { get; set; }
    }
}