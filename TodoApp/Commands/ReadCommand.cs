using System;
using TodoApp.Exceptions;
using TodoApp.Services;

namespace TodoApp.Commands
{
    public class ReadCommand : ICommand
    {
        private readonly int _index;

        public ReadCommand(int index)
        {
            _index = index;
        }

        public void Execute()
        {
            var todos = AppInfo.RequireCurrentTodoList();
            var item = todos[_index];

            if (item == null)
            {
                throw new TaskNotFoundException($"Задача с индексом {_index} не существует.");
            }

            Console.WriteLine(item.GetFullInfo());
        }
    }
}
