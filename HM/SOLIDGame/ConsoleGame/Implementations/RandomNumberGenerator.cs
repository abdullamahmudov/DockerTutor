using System;
using ConsoleGame.Interfaces;

namespace ConsoleGame.Implementations;
/// <summary>
/// Реализация генератора случайных чисел
/// </summary>
public class RandomNumberGenerator : IRandomNumberGenerator
{
    private readonly Random _random = new Random();
    /// <inheritdoc/>
    public int Generate(int min, int max)
    {
        return _random.Next(min, max);
    }
}