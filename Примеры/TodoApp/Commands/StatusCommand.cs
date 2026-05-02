using System;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp.Commands
{
	public class StatusCommand : IUndoableCommand
	{
		private int _index;
		private TodoStatus _newStatus;
		private TodoStatus _oldStatus;
		private TodoList _todos;

		public StatusCommand(int index, TodoStatus status)
		{
			_index = index;
			_newStatus = status;
		}

		public void Execute()
		{
			_todos = AppInfo.GetCurrentTodoList();
			if (_todos == null) return;


			var item = _todos[_index];

			if (item == null)
			{
				Console.WriteLine($"Ошибка: задача с индексом {_index} не найдена.");
				return;
			}

			_oldStatus = item.Status;
			_todos.SetStatus(_index, _newStatus);
			// Событие OnStatusChanged будет вызвано автоматически в TodoList.SetStatus()

			Console.WriteLine($"Статус задачи изменён на: {_newStatus}");
		}

		public void Unexecute()
		{
			_todos = AppInfo.GetCurrentTodoList();
			if (_todos == null) return;


			_todos.SetStatus(_index, _oldStatus);
			// Событие OnStatusChanged будет вызвано автоматически в TodoList.SetStatus()
			Console.WriteLine("Отменено изменение статуса");
		}
	}
}
