using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp.Commands
{
    public class SearchCommand : ICommand
    {
        private readonly string? _contains;
        private readonly string? _startsWith;
        private readonly string? _endsWith;
        private readonly DateTime? _from;
        private readonly DateTime? _to;
        private readonly TodoStatus? _status;
        private readonly string? _sort;
        private readonly bool _desc;
        private readonly int? _top;

        public SearchCommand(
            string? contains = null,
            string? startsWith = null,
            string? endsWith = null,
            DateTime? from = null,
            DateTime? to = null,
            TodoStatus? status = null,
            string? sort = null,
            bool desc = false,
            int? top = null)
        {
            _contains = contains;
            _startsWith = startsWith;
            _endsWith = endsWith;
            _from = from;
            _to = to;
            _status = status;
            _sort = sort;
            _desc = desc;
            _top = top;
        }

        public void Execute()
        {
            var todos = CommandParser.Todos;
            if (todos == null || todos.Count == 0)
            {
                Console.WriteLine("Ничего не найдено");
                return;
            }

            var query = todos.GetAll()
                .Select((item, index) => new SearchResult(index, item))
                .Where(result => string.IsNullOrEmpty(_contains)
                    || result.Item.Text.IndexOf(_contains, StringComparison.OrdinalIgnoreCase) >= 0)
                .Where(result => string.IsNullOrEmpty(_startsWith)
                    || result.Item.Text.StartsWith(_startsWith, StringComparison.OrdinalIgnoreCase))
                .Where(result => string.IsNullOrEmpty(_endsWith)
                    || result.Item.Text.EndsWith(_endsWith, StringComparison.OrdinalIgnoreCase))
                .Where(result => !_from.HasValue || result.Item.LastUpdate.Date >= _from.Value.Date)
                .Where(result => !_to.HasValue || result.Item.LastUpdate.Date <= _to.Value.Date)
                .Where(result => !_status.HasValue || result.Item.Status == _status.Value);

            query = ApplySort(query);

            if (_top.HasValue)
            {
                query = query.Take(_top.Value);
            }

            var results = query.ToList();
            if (results.Count == 0)
            {
                Console.WriteLine("Ничего не найдено");
                return;
            }

            Console.WriteLine(BuildTable(results));
        }

        private IEnumerable<SearchResult> ApplySort(IEnumerable<SearchResult> query)
        {
            if (_sort == "text")
            {
                return _desc
                    ? query.OrderByDescending(result => result.Item.Text).ThenBy(result => result.Index)
                    : query.OrderBy(result => result.Item.Text).ThenBy(result => result.Index);
            }

            if (_sort == "date")
            {
                return _desc
                    ? query.OrderByDescending(result => result.Item.LastUpdate).ThenBy(result => result.Index)
                    : query.OrderBy(result => result.Item.LastUpdate).ThenBy(result => result.Index);
            }

            return query.OrderBy(result => result.Index);
        }

        private string BuildTable(List<SearchResult> results)
        {
            var table = new StringBuilder();
            const int indexWidth = 5;
            const int textWidth = 35;
            const int statusWidth = 15;
            const int dateWidth = 20;

            table.AppendLine($"+-------+-------------------------------------+-----------------+----------------------+");
            table.AppendLine($"| {"Index".PadRight(indexWidth)} | {"Text".PadRight(textWidth)} | {"Status".PadRight(statusWidth)} | {"LastUpdate".PadRight(dateWidth)} |");
            table.AppendLine($"+-------+-------------------------------------+-----------------+----------------------+");

            table.AppendLine(string.Join(Environment.NewLine, results.Select(result =>
                $"| {result.Index.ToString().PadRight(indexWidth)} | {GetShortText(result.Item.Text).PadRight(textWidth)} | {result.Item.Status.ToString().PadRight(statusWidth)} | {result.Item.LastUpdate.ToString("yyyy-MM-dd HH:mm").PadRight(dateWidth)} |")));

            table.AppendLine($"+-------+-------------------------------------+-----------------+----------------------+");
            return table.ToString();
        }

        private string GetShortText(string text)
        {
            var singleLineText = text.Replace("\n", " ").Replace("\r", " ");
            return singleLineText.Length > 30
                ? singleLineText.Substring(0, 30) + "..."
                : singleLineText;
        }

        private class SearchResult
        {
            public int Index { get; }
            public TodoItem Item { get; }

            public SearchResult(int index, TodoItem item)
            {
                Index = index;
                Item = item;
            }
        }
    }
}
