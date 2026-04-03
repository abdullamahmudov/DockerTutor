using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp.Implementations
{
    /// <summary>
    /// Абстрактный базовый класс для фоновых задач с периодическим выполнением.
    /// </summary>
    public abstract class AFonTask
    {
        protected readonly FileReaderWriter _readerWriter;
        private readonly int _delay;
        private bool _stoped = false;

        /// <summary>
        /// Флаг остановки задачи.
        /// </summary>
        public bool Stop
        {
            get
            {
                lock (_stopLocker)
                    return _stoped;
            }
            set
            {
                lock (_stopLocker)
                    _stoped = value;
            }
        }
        private object _stopLocker = new object();
        protected bool _isRun = false;
        protected Thread? _thread;

        /// <summary>
        /// Абстрактный метод для выполнения основной логики задачи.
        /// </summary>
        protected abstract void Process();

        /// <summary>
        /// Инициализирует новый экземпляр AFonTask.
        /// </summary>
        /// <param name="readerWriter">Компонент для чтения/записи файлов.</param>
        /// <param name="delay">Задержка между выполнениями в миллисекундах.</param>
        public AFonTask(FileReaderWriter readerWriter, int delay)
        {
            _readerWriter = readerWriter;
            _delay = delay;
        }

        /// <summary>
        /// Запускает задачу в фоновом потоке.
        /// </summary>
        public void Run()
        {
            if (_isRun)
            {
                return;
            }
            _isRun = true;
            _thread = new Thread(RunnedTask);
            _thread.Start();
        }

        /// <summary>
        /// Основной цикл выполнения задачи в потоке.
        /// </summary>
        protected void RunnedTask()
        {
            do
            {
                Process();
                Thread.Sleep(_delay);
            } while (!Stop);
            Stop = false;
            _isRun = false;
        }

        /// <summary>
        /// Асинхронно ожидает завершения задачи.
        /// </summary>
        /// <returns>Задача, завершающаяся при остановке потока.</returns>
        public async Task WaitStoped()
        {
            if (_thread is null)
                return;

            while (_thread.ThreadState != ThreadState.Stopped)
            {
                await Task.Delay(100);
            }
        }
    }
}