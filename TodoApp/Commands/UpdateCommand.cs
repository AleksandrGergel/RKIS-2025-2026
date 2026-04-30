using System;
using TodoApp.Exceptions;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp.Commands
{
    public class UpdateCommand : IUndoableCommand
    {
        private readonly int _index;
        private readonly string _newText;
        private string? _oldText;
        private TodoList? _todos;

        public UpdateCommand(int index, string newText)
        {
            _index = index;
            _newText = newText;
        }

        public void Execute()
        {
            if (string.IsNullOrWhiteSpace(_newText))
            {
                throw new InvalidArgumentException("Текст задачи не может быть пустым.");
            }

            _todos = AppInfo.RequireCurrentTodoList();
            var item = _todos[_index];

            if (item == null)
            {
                throw new TaskNotFoundException($"Задача с индексом {_index} не существует.");
            }

            _oldText = item.Text;
            _todos.UpdateItem(_index, _newText);
            Console.WriteLine("Задача обновлена.");
        }

        public void Unexecute()
        {
            _todos = AppInfo.RequireCurrentTodoList();
            if (_oldText == null) return;

            _todos.UpdateItem(_index, _oldText);
            Console.WriteLine("Отменено обновление задачи");
        }
    }
}
