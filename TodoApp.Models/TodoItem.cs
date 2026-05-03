using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoApp.Models
{
    public class TodoItem
    {
        private readonly IClock _clock;

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string Text { get; set; }

        public TodoStatus Status { get; set; }

        public DateTime LastUpdate { get; set; }

        public Guid ProfileId { get; set; }

        public Profile? Profile { get; set; }

        public TodoItem()
            : this(string.Empty, new SystemClock())
        {
        }

        public TodoItem(string text, IClock? clock = null)
        {
            _clock = clock ?? new SystemClock();
            Text = text;
            Status = TodoStatus.NotStarted;
            LastUpdate = _clock.Now;
        }

        public void UpdateText(string newText)
        {
            Text = newText;
            LastUpdate = _clock.Now;
        }

        public void SetStatus(TodoStatus status)
        {
            Status = status;
            LastUpdate = _clock.Now;
        }

        [NotMapped]
        public string ShortInfo => GetShortInfo();

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
