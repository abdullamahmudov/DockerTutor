using System.Diagnostics;
using ParallelFileReader.Interfaces;

namespace ParallelFileReader;

public class Reader : IReader
{
    /// <inheritdoc/>
    public async Task<string[]> ReadFilesFromDirectory(string pathToDirectory, int limitFiles)
    {
        var stopwatch = new Stopwatch();
        if (!Directory.Exists(pathToDirectory))
        {
            throw new DirectoryNotFoundException();
        }

        var files = Directory.GetFiles(pathToDirectory);
        var tasks = new List<Task<string>>();
        stopwatch.Start();
        var count = 0;
        foreach (var path in files)
        {
            tasks.Add(Task.Run<string>(() => Read(path)));
            count++;
            if (count >= limitFiles)
                break;
        }
        var texts = await Task.WhenAll(tasks);
        stopwatch.Stop();
        Console.WriteLine(string.Concat("ВРЕМЯ ЧТЕНИЯ ФАЙЛОВ: ", stopwatch.ElapsedMilliseconds, " milliseconds","\n= = = = = = = = = = "));
        Console.WriteLine();
        return texts;
    }

    internal async Task<string> Read(string path)
    {
        return await File.ReadAllTextAsync(path);
    }
}
