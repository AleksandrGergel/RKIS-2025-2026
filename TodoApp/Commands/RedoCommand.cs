using TodoApp.Exceptions;
using TodoApp.Services;

namespace TodoApp.Commands
{
    public class RedoCommand : ICommand
    {
        public void Execute()
        {
            AppInfo.RequireCurrentTodoList();

            if (AppInfo.RedoStack.Count == 0)
            {
                throw new InvalidCommandException("Нечего повторять.");
            }

            var command = AppInfo.RedoStack.Pop();
            command.Execute();
            AppInfo.UndoStack.Push(command);
        }
    }
}
