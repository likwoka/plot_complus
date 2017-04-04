using System;


namespace PlotComPlus.Logging
{
    /// <summary>
    /// Output INFO message to the console.
    /// </summary>
    class ConsoleLogger : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
