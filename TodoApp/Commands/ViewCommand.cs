using System;
using TodoApp.Services;

namespace TodoApp.Commands
{
    public class ViewCommand : ICommand
    {
        private readonly bool _showIndex;
        private readonly bool _showStatus;
        private readonly bool _showDate;

        public ViewCommand(bool showIndex = false, bool showStatus = false, bool showDate = false)
        {
            _showIndex = showIndex;
            _showStatus = showStatus;
            _showDate = showDate;
        }

        public void Execute()
        {
            var todos = AppInfo.RequireCurrentTodoList();
            if (todos.Count == 0)
            {
                Console.WriteLine("Список задач пуст.");
                return;
            }

            Console.WriteLine(todos.GetTable(_showIndex, _showStatus, _showDate));
        }
    }
}
