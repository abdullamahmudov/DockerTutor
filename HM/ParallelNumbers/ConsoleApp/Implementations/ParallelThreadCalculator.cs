using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsoleApp.Interfaces;

namespace ConsoleApp.Implementations
{
    /// <summary>
    /// Параллельный метод посчета через потоки
    /// </summary>
    public class ParallelThreadCalculator : ICalculator
    {
        /// <inheritdoc/>
        public string Name => "Параллельный через потоки(четные/нечетные))";

        public bool IsRunning { get; private set; } = false;
        private int? _evenThreadResult;
        private object _evenThreadLocker = new object();
        private int? EvenThreadResult
        {
            get
            {
                lock(_evenThreadLocker)
                {
                    return _evenThreadResult;
                }
            }
            set
            {
                lock(_evenThreadLocker)
                {
                    _evenThreadResult = value;
                }
            }
        }
        private int? _oddThreadResult;
        private object _oddThreadLocker = new object();
        private int? OddThreadResult
        {
            get
            {
                lock(_oddThreadLocker)
                {
                    return _oddThreadResult;
                }
            }
            set
            {
                lock(_oddThreadLocker)
                {
                    _oddThreadResult = value;
                }
            }
        }
        private object _locker = new object();
        private List<int>? _list;
        private List<int>? LockedList
        {
            get
            {
                lock (_locker)
                {
                    return _list;
                }
            }
            set => _list = value;
        }

        /// <inheritdoc/>
        public async Task<int> GetSum(List<int> list)
        {
            if (IsRunning)
            {
                throw new Exception("Экземпляр метода поддерживает лишь одно единовеременное выполнение операции!");
            }

            IsRunning = true;

            LockedList = list;
            var evenThread = new Thread(() => CalculateSum(0, list.Count, (index) => index + 2, (result) => EvenThreadResult = result));
            evenThread.Start();

            var oddThread = new Thread(() => CalculateSum(1, list.Count, (index) => index + 2, (result) => OddThreadResult = result));
            oddThread.Start();

            while(EvenThreadResult is null || OddThreadResult is null)
            {
                await Task.Delay(100);
            }

            var sum = EvenThreadResult.Value + OddThreadResult.Value;

            LockedList = null;
            EvenThreadResult = null;
            OddThreadResult = null;
            IsRunning = false;

            return sum;
        }

        /// <summary>
        /// Вычисление суммы
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <param name="indexCalculator"></param>
        /// <param name="resultWriter"></param>
        /// <returns></returns>
        private void CalculateSum(int startIndex, int length, Func<int, int> indexCalculator, Action<int> resultWriter)
        {
            if (LockedList is null)
                throw new NullReferenceException("Список не определен!");

            var sum = 0;
            for (int i = startIndex; i < length; i = indexCalculator(i))
            {
                sum += LockedList[i];
            }
            resultWriter(sum);
        }
    }
}