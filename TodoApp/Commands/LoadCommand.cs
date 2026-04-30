using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TodoApp.Commands
{
    public class LoadCommand : ICommand
    {
        private static readonly object _consoleLock = new object();
        private static readonly Random _random = new Random();

        private readonly int _downloadsCount;
        private readonly int _downloadSize;
        private int _startRow;

        public LoadCommand(int downloadsCount, int downloadSize)
        {
            _downloadsCount = downloadsCount;
            _downloadSize = downloadSize;
        }

        public void Execute()
        {
            RunAsync().Wait();
        }

        private async Task RunAsync()
        {
            _startRow = Console.CursorTop;

            for (int i = 0; i < _downloadsCount; i++)
            {
                Console.WriteLine();
            }

            var tasks = new List<Task>();
            for (int i = 0; i < _downloadsCount; i++)
            {
                int index = i;
                tasks.Add(DownloadAsync(index));
            }

            await Task.WhenAll(tasks);

            lock (_consoleLock)
            {
                Console.SetCursorPosition(0, _startRow + _downloadsCount);
                Console.WriteLine("Все загрузки завершены.");
            }
        }

        private async Task DownloadAsync(int index)
        {
            for (int loaded = 0; loaded <= _downloadSize; loaded++)
            {
                int percent = loaded * 100 / _downloadSize;
                UpdateProgressBar(index, percent);
                await Task.Delay(GetDelay());
            }
        }

        private void UpdateProgressBar(int index, int percent)
        {
            const int width = 20;
            int filled = percent / 5;
            string bar = $"Загрузка {index + 1}: [{new string('#', filled)}{new string('-', width - filled)}] {percent}%";

            lock (_consoleLock)
            {
                Console.SetCursorPosition(0, _startRow + index);
                Console.Write(bar.PadRight(Console.WindowWidth - 1));
            }
        }

        private static int GetDelay()
        {
            lock (_random)
            {
                return _random.Next(20, 90);
            }
        }
    }
}
