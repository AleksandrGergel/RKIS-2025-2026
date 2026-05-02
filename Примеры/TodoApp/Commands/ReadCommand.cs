using System;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp.Commands
{
	public class ReadCommand : ICommand
	{
		private int _index;

		public ReadCommand(int index)
		{
			_index = index;
		}

		public void Execute()
		{
			var todos = AppInfo.GetCurrentTodoList();
			if (todos == null) return;

			var item = todos[_index];

			if (item == null)
			{
				Console.WriteLine($"Ошибка: задача с индексом {_index} не найдена.");
				return;
			}

			Console.WriteLine(item.GetFullInfo());
		}
	}
}
