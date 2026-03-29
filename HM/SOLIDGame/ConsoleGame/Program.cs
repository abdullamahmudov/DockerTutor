using System;
using ConsoleGame.Interfaces;
using ConsoleGame.Implementations;

namespace ConsoleGame;

class Program
{
    static void Main(string[] args)
    {
        // Создание зависимостей (DIP)
        IRandomNumberGenerator numberGenerator = new RandomNumberGenerator();
        IInputOutput inputOutput = new ConsoleInputOutput();
        var configurator = new RulesConfigurator(inputOutput);
        if(!configurator.Configure()) return;

        var game = new GuessNumberGame(numberGenerator, inputOutput, configurator.MinimalNumber, configurator.MaximalNumber);
        game.Play();
    }
}