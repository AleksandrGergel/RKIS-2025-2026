using System;

namespace TodoApp.Models
{
    public class TodoItem
    {
        public string Text { get; private set; }
        public TodoStatus Status { get; set; }
        public DateTime LastUpdate { get; set; }

        public TodoItem(string text)
        {
            Text = text;
            Status = TodoStatus.NotStarted;
            LastUpdate = DateTime.Now;
        }

        public void UpdateText(string newText)
        {
            Text = newText;
            LastUpdate = DateTime.Now;
        }

        public void SetStatus(TodoStatus status)
        {
            Status = status;
            LastUpdate = DateTime.Now;
        }

        public string GetShortInfo()
        {
			string shortText = Text.Length > 30 
                ? Text.Replace("\n", " ").Substring(0, 30) + "..." 
                : Text;
            return shortText;
        }

        public string GetFullInfo()
        {
            return $"Текст: {Text}\nСтатус: {Status}\nПоследнее изменение: {LastUpdate:yyyy-MM-dd HH:mm:ss}";
        }
    }
}
