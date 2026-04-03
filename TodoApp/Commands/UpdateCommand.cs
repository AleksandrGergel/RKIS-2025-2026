using System;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp.Commands
{
	public class UpdateCommand : IUndoableCommand
	{
		private int _index;
		private string _newText;
		private string _oldText;
		private TodoList _todos;

		public UpdateCommand(int index, string newText)
		{
			_index = index;
			_newText = newText;
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

			_oldText = item.Text;
			_todos.UpdateItem(_index, _newText);
			// Событие OnTodoUpdated будет вызвано автоматически в TodoList.UpdateItem()

			Console.WriteLine($"Задача обновлена.");
		}

		public void Unexecute()
		{
			_todos = AppInfo.GetCurrentTodoList();
			if (_todos == null || _oldText == null) return;

			_todos.UpdateItem(_index, _oldText);
			// Событие OnTodoUpdated будет вызвано автоматически в TodoList.UpdateItem()

			Console.WriteLine("Отменено обновление задачи");
		}
	}
}
