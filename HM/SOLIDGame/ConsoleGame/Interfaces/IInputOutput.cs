using System;

namespace ConsoleGame.Interfaces;
/// <summary>
/// Интерфейс для ввода/вывода (ISP, DIP)
/// </summary>
public interface IInputOutput
{
    /// <summary>
    /// Написать соощение в новой строке
    /// </summary>
    /// <param name="message"></param>
    void WriteLine(string message);
    /// <summary>
    /// Написать сообщение
    /// </summary>
    /// <param name="message"></param>
    void Write(string message);
    /// <summary>
    /// Прочитать сообщение
    /// </summary>
    /// <returns></returns>
    string ReadLine();
}