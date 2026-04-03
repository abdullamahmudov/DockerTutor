using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp.Implementations
{
    /// <summary>
    /// Компонент для обработки ввода команд пользователя и вывода меню.
    /// </summary>
    public class InputOutput
    {
        private const string All_COMMANDS_TEXT =
            "1. Запусти прослушивание каталога файлов\n" +
            "2. Запустить запись случайных объектов в каталог\n" +
            "3. Остановить прослушивание каталога файлов\n" +
            "4. Остановить запись случайных файлов\n" +
            "5. Завершение программы\n";
        private readonly ProcessManager _processManager;

        /// <summary>
        /// Инициализирует новый экземпляр InputOutput.
        /// </summary>
        /// <param name="processManager">Менеджер процессов для управления задачами.</param>
        public InputOutput(ProcessManager processManager)
        {
            _processManager = processManager;
        }

        /// <summary>
        /// Ожидает команды от пользователя в цикле и выполняет соответствующие действия.
        /// </summary>
        public void WaitCommands()
        {
            int? command = null;
            do
            {
                WriteAllCommands();
                command = ReadCommand();
                switch (command)
                {
                    case 1:
                        _processManager.RunFileChecker();
                        break;
                    case 2:
                        _processManager.RunObjectWriter();
                        break;
                    case 3:
                        _processManager.StopChecker();
                        break;
                    case 4:
                        _processManager.StopObjectWriter();
                        break;
                    default:
                        break;
                }
            } while (command != 5);
            _processManager.StopedAll().Wait();
        }

        /// <summary>
        /// Выводит список всех доступных команд в консоль.
        /// </summary>
        private void WriteAllCommands()
        {
            Console.WriteLine(All_COMMANDS_TEXT);
        }

        /// <summary>
        /// Читает команду из консоли и преобразует в число.
        /// </summary>
        /// <returns>Номер команды или null, если ввод некорректен.</returns>
        private int? ReadCommand()
        {
            var input = Console.ReadLine();
            if (!int.TryParse(input, out var command))
            {
                return null;
            }
            return command;
        }
    }
}