using System;
using System.Collections.Generic;
using System.Text;

namespace TodoApp.Models
{
	public class TodoList
	{
		private List<TodoItem> _items;

		// События
		public event Action<TodoItem> OnTodoAdded;
		public event Action<TodoItem> OnTodoDeleted;
		public event Action<TodoItem> OnTodoUpdated;
		public event Action<TodoItem> OnStatusChanged;

		public TodoList()
		{
			_items = new List<TodoItem>();
		}

		public void Add(TodoItem item)
		{
			_items.Add(item);
			OnTodoAdded?.Invoke(item);
		}

		public void Delete(int index)
		{
			if (index >= 0 && index < _items.Count)
			{
				var item = _items[index];
				_items.RemoveAt(index);
				OnTodoDeleted?.Invoke(item);
			}
		}

		public TodoItem GetItem(int index)
		{
			if (index >= 0 && index < _items.Count)
			{
				return _items[index];
			}
			return null;
		}

		public void SetStatus(int index, TodoStatus status)
		{
			if (index >= 0 && index < _items.Count)
			{
				_items[index].SetStatus(status);
				OnStatusChanged?.Invoke(_items[index]);
			}
		}

		public void UpdateItem(int index, string newText)
		{
			if (index >= 0 && index < _items.Count)
			{
				_items[index].UpdateText(newText);
				OnTodoUpdated?.Invoke(_items[index]);
			}
		}

		public int Count => _items.Count;

		public TodoItem this[int index]
		{
			set
			{
				if (index >= 0 && index < _items.Count)
				{
					_items[index] = value;
					OnTodoUpdated?.Invoke(value);
				}
			}
			get
			{
				return GetItem(index);
			}
		}

		public IEnumerator<TodoItem> GetEnumerator()
		{
			foreach (var item in _items)
			{
				yield return item;
			}
		}

		public List<TodoItem> GetAll()
		{
			return _items;
		}
		public string GetTable(bool _showIndex = true, bool _showStatus = true, bool _showDate = true)
		{
			var table = new StringBuilder();
			int indexWidth = 5;
			int textWidth = 35;
			int statusWidth = 15;
			int dateWidth = 20;

			// Вычисляем общую ширину таблицы
			var columns = new List<int>();
			var headers = new List<string>();

			if (_showIndex)
			{
				columns.Add(indexWidth);
				headers.Add("INDEX");
			}

			columns.Add(textWidth);
			headers.Add("ЗАДАЧА");

			if (_showStatus) 
			{ 
				columns.Add(statusWidth); 
				headers.Add("СТАТУС"); 
			}
			if (_showDate) 
			{ 
				columns.Add(dateWidth); 
				headers.Add("ДАТА"); 
			}

			// Функция для построения горизонтальной линии
			string BuildLine(char left, char mid, char right, char fill)
			{
				var sb = new StringBuilder();
				sb.Append(left);
				for (int i = 0; i < columns.Count; i++)
				{
					sb.Append(new string(fill, columns[i] + 2));
					sb.Append(i < columns.Count - 1 ? mid : right);
				}
				return sb.ToString();
			}

			// Верхняя граница: ╔═══╦═══╗
			table.AppendLine(BuildLine('╔', '╦', '╗', '═'));

			// Строка заголовков: ║ TEXT ║ TEXT ║
			table.Append('║');
			for (int i = 0; i < columns.Count; i++)
				table.Append($" {headers[i].PadRight(columns[i])} ║");
			table.AppendLine();

			// Разделитель после заголовка: ╠═══╬═══╣
			table.AppendLine(BuildLine('╠', '╬', '╣', '═'));

			// Строки данных
			int index = 0;
			foreach (var item in _items)
			{
				table.Append('║');
				int col = 0;

				if (_showIndex)
					table.Append($" {index.ToString().PadRight(columns[col++])} ║");

				string shortText = item.GetShortInfo();
				table.Append($" {shortText.PadRight(columns[col++])} ║");

				if (_showStatus)
					table.Append($" {item.Status.ToString().PadRight(columns[col++])} ║");

				if (_showDate)
					table.Append($" {item.LastUpdate.ToString("yyyy-MM-dd HH:mm").PadRight(columns[col++])} ║");

				table.AppendLine();

				// Разделитель между строками (кроме последней)
				if (index < _items.Count - 1)
					table.AppendLine(BuildLine('╠', '╬', '╣', '═'));

				index++;
			}

			// Нижняя граница: ╚═══╩═══╝
			table.AppendLine(BuildLine('╚', '╩', '╝', '═'));

			return table.ToString();
		}

	}
}
