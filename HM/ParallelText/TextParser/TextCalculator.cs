using System.Diagnostics;
using TextParser.Interfaces;

namespace TextParser;

public class TextCalculator : ITextCalculator
{
    /// <inheritdoc/>
    public Task CalculateSpaces(string text)
    {
        var stopwatch = new Stopwatch();
        var count = 0;
        stopwatch.Start();
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] != ' ')
                continue;

            count++;
        }
        stopwatch.Stop();
        Console.WriteLine(string.Concat("КОЛЛИЧЕСТВО ПРОБЕЛОВ В ТЕКСТЕ: ", count,"\nВРЕМЯ ПОДСЧЕТА ПРОБЕЛОВ: ", stopwatch.ElapsedMilliseconds, " milliseconds","\n- - - - - - - - - - - - -"));
        return Task.CompletedTask;
    }
}
