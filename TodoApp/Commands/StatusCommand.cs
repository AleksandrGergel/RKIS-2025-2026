using System;
using TodoApp.Exceptions;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp.Commands
{
    public class StatusCommand : IUndoableCommand
    {
        private readonly int _index;
        private readonly TodoStatus _newStatus;
        private TodoStatus _oldStatus;
        private TodoList? _todos;

        public StatusCommand(int index, TodoStatus status)
        {
            _index = index;
            _newStatus = status;
        }

        public void Execute()
        {
            _todos = AppInfo.RequireCurrentTodoList();
            var item = _todos[_index];

            if (item == null)
            {
                throw new TaskNotFoundException($"Задача с индексом {_index} не существует.");
            }

            _oldStatus = item.Status;
            _todos.SetStatus(_index, _newStatus);
            Console.WriteLine($"Статус задачи изменён на: {_newStatus}");
        }

        public void Unexecute()
        {
            _todos = AppInfo.RequireCurrentTodoList();
            _todos.SetStatus(_index, _oldStatus);
            Console.WriteLine("Отменено изменение статуса");
        }
    }
}
