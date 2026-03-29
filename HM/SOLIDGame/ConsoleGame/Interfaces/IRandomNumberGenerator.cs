using System;

namespace ConsoleGame.Interfaces;
// Интерфейс для генерации случайного числа (DIP)
public interface IRandomNumberGenerator
{
    /// <summary>
    /// Генерация случайного числа
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    int Generate(int min, int max);
}