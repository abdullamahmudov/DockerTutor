
using ConsoleApp.Implementations;

var processManager = new ProcessManager();
var inputOutput = new InputOutput(processManager);
inputOutput.WaitCommands();
