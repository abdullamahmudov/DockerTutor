using System;
using ConsoleGame.Interfaces;

namespace ConsoleGame.Implementations;
/// <summary>
/// Реализация ввода/вывода для консоли
/// </summary>
public class ConsoleInputOutput : IInputOutput
{
    /// <inheritdoc/>
    public void WriteLine(string message) => Console.WriteLine(message);
    /// <inheritdoc/>
    public void Write(string message) => Console.Write(message);
    /// <inheritdoc/>
    public string ReadLine() => Console.ReadLine() ?? string.Empty;
}