using System;
using ConsoleGame.Interfaces;

namespace ConsoleGame.Implementations;
/// <summary>
/// Класс игры с единственной ответственностью (SRP)
/// </summary>
internal class GuessNumberGame
{
    private readonly IRandomNumberGenerator _numberGenerator;
    private readonly IInputOutput _inputOutput;
    private readonly int _min;
    private readonly int _max;

    internal GuessNumberGame(IRandomNumberGenerator numberGenerator, IInputOutput inputOutput, int min, int max)
    {
        _numberGenerator = numberGenerator;
        _inputOutput = inputOutput;
        _min = min;
        _max = max;
    }

    /// <summary>
    /// Запуск игры
    /// </summary>
    internal void Play()
    {
        int secretNumber = _numberGenerator.Generate(_min, _max + 1);
        int attempts = 0;
        bool guessed = false;

        _inputOutput.WriteLine($"Добро пожаловать в игру 'Угадай число'!");
        _inputOutput.WriteLine($"Я загадал число от {_min} до {_max}. Попробуй угадать!");

        while (!guessed)
        {
            _inputOutput.Write("Введи своё предположение: ");
            string input = GetValidInput();
            if (int.TryParse(input, out int guess))
            {
                attempts++;
                if (guess < secretNumber)
                {
                    _inputOutput.WriteLine("Моё число больше!");
                }
                else if (guess > secretNumber)
                {
                    _inputOutput.WriteLine("Моё число меньше!");
                }
                else
                {
                    _inputOutput.WriteLine($"Поздравляю! Ты угадал число {secretNumber} за {attempts} попыток.");
                    guessed = true;
                }
            }
            else
            {
                _inputOutput.WriteLine("Пожалуйста, введи целое число.");
            }
        }

        _inputOutput.WriteLine("Спасибо за игру!");
    }

    /// <summary>
    /// Получение валидного значения
    /// </summary>
    /// <returns></returns>
    private string GetValidInput()
    {
        string input = string.Empty;
        while (string.IsNullOrWhiteSpace(input))
        {
            input = _inputOutput.ReadLine();
        }
        return input;
    }
}