using TodoApp.Exceptions;
using TodoApp.Services;

namespace TodoApp.Commands
{
    public class UndoCommand : ICommand
    {
        public void Execute()
        {
            AppInfo.RequireCurrentTodoList();

            if (AppInfo.UndoStack.Count == 0)
            {
                throw new InvalidCommandException("Нечего отменять.");
            }

            var command = AppInfo.UndoStack.Pop();
            command.Unexecute();
            AppInfo.RedoStack.Push(command);
        }
    }
}
