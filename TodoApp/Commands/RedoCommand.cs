using System;
using TodoApp.Services;

namespace TodoApp.Commands
{
    public class RedoCommand : ICommand
    {
        public void Execute()
        {
            if (AppInfo.RedoStack.Count == 0)
            {
                Console.WriteLine("Нечего повторять.");
                return;
            }

            var command = AppInfo.RedoStack.Pop();
            command.Execute();
            AppInfo.UndoStack.Push(command);
        }
    }
}
