using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsoleApp.Models;

namespace ConsoleApp.Implementations
{
    /// <summary>
    /// Управляет процессами мониторинга файлов, записи объектов и обработки событий.
    /// Координирует работу слушателей, логгеров и исполнителей.
    /// </summary>
    public class ProcessManager
    {
        private readonly FileInfoListener _fileReadLogListener;
        private readonly FileInfoListener _maxValueExecuteListener;

        private readonly FileReaderWriter _logsReadWriter;
        private readonly Logger _logger;
        private readonly FileMonitorLogger _fileMonitorLogger;
        private readonly MaxValueExecuter<ExampleClass, ExampleClassElement> _maxValueExecuter;

        private readonly FileReaderWriter _filesReadWriter;
        private readonly FileMonitor _fileMonitor;
        private readonly ExampleObjectWriter _objectWriter;

        /// <summary>
        /// Инициализирует новый экземпляр класса ProcessManager.
        /// Настраивает все необходимые компоненты: логгеры, мониторы, писатели и слушатели.
        /// </summary>
        public ProcessManager()
        {
            _logsReadWriter = new FileReaderWriter();
            _logger = new Logger(_logsReadWriter);
            _fileMonitorLogger = new FileMonitorLogger(_logger);

            _filesReadWriter = new FileReaderWriter();
            _maxValueExecuter = new MaxValueExecuter<ExampleClass, ExampleClassElement>(
                _logger,
                _filesReadWriter,
                ExampleClass.ExecuteEnumerable,
                ExampleClassElement.ExecuteWeight);

            _fileMonitor = new FileMonitor(_filesReadWriter, 1000);
            _objectWriter = new ExampleObjectWriter(_filesReadWriter, 10000);

            _fileReadLogListener = new FileInfoListener(_fileMonitorLogger);
            _fileReadLogListener.Subscribe(_fileMonitor);
            _maxValueExecuteListener = new FileInfoListener(_maxValueExecuter);
            _maxValueExecuteListener.Subscribe(_fileMonitor);
        }

        /// <summary>
        /// Запускает процесс мониторинга файлов в фоновом режиме.
        /// </summary>
        public void RunFileChecker() => _fileMonitor.Run();

        /// <summary>
        /// Останавливает процесс мониторинга файлов.
        /// </summary>
        public void StopChecker() => _fileMonitor.Stop = true;

        /// <summary>
        /// Запускает процесс записи случайных объектов в файлы в фоновом режиме.
        /// </summary>
        public void RunObjectWriter() => _objectWriter.Run();

        /// <summary>
        /// Останавливает процесс записи объектов в файлы.
        /// </summary>
        public void StopObjectWriter() => _objectWriter.Stop = true;

        /// <summary>
        /// Асинхронно останавливает все запущенные процессы и ожидает их завершения.
        /// Отписывает слушателей от событий и завершает задачи.
        /// </summary>
        /// <returns>Задача, представляющая асинхронную операцию остановки.</returns>
        public async Task StopedAll()
        {
            _fileReadLogListener.Unsubscribe(_fileMonitor);
            _maxValueExecuteListener.Unsubscribe(_fileMonitor);
            StopChecker();
            StopObjectWriter();
            await Task.WhenAll(_fileMonitor.WaitStoped(), _objectWriter.WaitStoped());
        }
    }
}