using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp.Implementations
{
    /// <summary>
    /// Компонент для чтения и записи файлов с потокобезопасностью.
    /// </summary>
    public class FileReaderWriter
    {
        /// <summary>
        /// Путь к директории файлов.
        /// </summary>
        public string PathToFiles => Path.Combine(AppContext.BaseDirectory, "filesFolder");

        /// <summary>
        /// Путь к директории логов.
        /// </summary>
        public string PathToLogs => Path.Combine(AppContext.BaseDirectory, "logs");

        private object readWriteLocker = new object();

        /// <summary>
        /// Получает пути всех файлов в указанной директории.
        /// </summary>
        /// <param name="directoryPath">Путь к директории.</param>
        /// <returns>Массив путей файлов или пустой массив, если директория не существует.</returns>
        public string[] GetFilesPath(string directoryPath)
        {
            lock (readWriteLocker)
            {
                if (!Directory.Exists(directoryPath))
                {
                    return [];
                }
                return Directory.GetFiles(directoryPath);
            }
        }

        /// <summary>
        /// Читает содержимое файла.
        /// </summary>
        /// <param name="filePath">Путь к файлу.</param>
        /// <returns>Содержимое файла или null, если файл не существует.</returns>
        public string? ReadFile(string filePath)
        {
            lock (readWriteLocker)
            {
                if (!File.Exists(filePath))
                    return null;

                return File.ReadAllText(filePath);
            }
        }

        /// <summary>
        /// Удаляет файл без блокировки.
        /// </summary>
        /// <param name="path">Путь к файлу.</param>
        /// <returns>True, если файл удален успешно или не существует.</returns>
        private bool RemoveFile(string path)
        {
            if (!File.Exists(path))
                return true;

            try
            {
                File.Delete(path);
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Удаляет файл с блокировкой.
        /// </summary>
        /// <param name="path">Путь к файлу.</param>
        /// <returns>True, если файл удален успешно.</returns>
        public bool RemoveFileLocked(string path)
        {
            lock (readWriteLocker)
            {
                return RemoveFile(path);
            }
        }

        /// <summary>
        /// Записывает текст в файл, добавляя к существующему содержимому.
        /// </summary>
        /// <param name="directoryPath">Путь к директории.</param>
        /// <param name="fileName">Имя файла.</param>
        /// <param name="text">Текст для записи.</param>
        public void WriteInFile(string directoryPath, string fileName, string text)
        {
            lock (readWriteLocker)
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                var filePath = Path.Combine(directoryPath, fileName);

                File.WriteAllText(filePath, text);
            }
        }

        /// <summary>
        /// Записывает текст в новый файл, удаляя существующий.
        /// </summary>
        /// <param name="directoryPath">Путь к директории.</param>
        /// <param name="fileName">Имя файла.</param>
        /// <param name="text">Текст для записи.</param>
        public void WriteFile(string directoryPath, string fileName, string text)
        {
            lock (readWriteLocker)
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                var filePath = Path.Combine(directoryPath, fileName);

                RemoveFileLocked(filePath);
                File.WriteAllText(filePath, text);
            }
        }
    }
}