using System;
using System.Text;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp.Commands
{
    public class ViewCommand : ICommand
    {
        private bool _showIndex;
        private bool _showStatus;
        private bool _showDate;

        public ViewCommand(bool showIndex = false, bool showStatus = false, bool showDate = false)
        {
            _showIndex = showIndex;
            _showStatus = showStatus;
            _showDate = showDate;
        }

        public void Execute()
        {
            var todos = AppInfo.GetCurrentTodoList();
            if (todos == null || todos.Count == 0)
            {
                Console.WriteLine("Список задач пуст.");
                return;
            }

			Console.WriteLine(todos.GetTable(_showIndex, _showStatus, _showDate));
		}


    }
}
