using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsoleGame.Interfaces;

namespace ConsoleGame.Implementations
{
    /// <summary>
    /// Класс конфигурации правил
    /// </summary>
    internal class RulesConfigurator
    {
        internal int MinimalNumber { get; set; }
        internal int MaximalNumber { get; set; }
        private readonly IInputOutput _inputOutput;
        internal RulesConfigurator(IInputOutput inputOutput)
        {
            _inputOutput = inputOutput;
        }
        /// <summary>
        /// Конфигурация правил
        /// </summary>
        /// <returns></returns>
        internal bool Configure()
        {
            MinimalNumber = GetValidNumber(_inputOutput, "Введите минимальное число диапазона: ");
            MaximalNumber = GetValidNumber(_inputOutput, "Введите максимальное число диапазона: ");
            if (MinimalNumber >= MaximalNumber)
            {
                _inputOutput.WriteLine("Минимальное число должно быть меньше максимального.");
                return false;
            }
            return true;
        }
        /// <summary>
        /// Получить валидное целое число
        /// </summary>
        /// <param name="io"></param>
        /// <param name="prompt"></param>
        /// <returns></returns>
        private int GetValidNumber(IInputOutput io, string prompt)
        {
            while (true)
            {
                io.Write(prompt);
                string input = io.ReadLine();
                if (int.TryParse(input, out int value))
                {
                    return value;
                }
                io.WriteLine("Пожалуйста, введите целое число.");
            }
        }
    }
}