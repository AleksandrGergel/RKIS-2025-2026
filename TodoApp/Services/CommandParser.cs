using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TodoApp.Commands;
using TodoApp.Models;

namespace TodoApp.Services
{
    public static class CommandParser
    {
        private static Dictionary<string, Func<string, ICommand>> _commandHandlers;
        public static TodoList? Todos => AppInfo.GetCurrentTodoList();

        static CommandParser()
        {
            InitializeHandlers();
        }

        private static void InitializeHandlers()
        {
            _commandHandlers = new Dictionary<string, Func<string, ICommand>>
            {
                ["help"] = args => new HelpCommand(),
                ["profile"] = args => ParseProfileCommand(SplitCommand(args)),
                ["add"] = args => ParseAddCommand(SplitCommand(args)),
                ["view"] = args => ParseViewCommand(SplitCommand(args)),
                ["read"] = args => ParseReadCommand(SplitCommand(args)),
                ["search"] = args => ParseSearchCommand(SplitCommand(args)),
                ["status"] = args => ParseStatusCommand(SplitCommand(args)),
                ["update"] = args => ParseUpdateCommand(SplitCommand(args)),
                ["delete"] = args => ParseDeleteCommand(SplitCommand(args)),
                ["undo"] = args => new UndoCommand(),
                ["redo"] = args => new RedoCommand(),
            };
        }

        public static ICommand Parse(string inputString)
        {
            if (string.IsNullOrWhiteSpace(inputString))
            {
                return new HelpCommand();
            }

            var trimmedInput = inputString.Trim();
            var commandMatch = Regex.Match(trimmedInput, @"^\S+");
            if (!commandMatch.Success)
                return new HelpCommand();

            string command = commandMatch.Value.ToLower();
            string args = trimmedInput.Substring(commandMatch.Length).TrimStart();

            if (_commandHandlers.ContainsKey(command))
            {
                try
                {
                    return _commandHandlers[command](args);
                }
                catch
                {
                    Console.WriteLine($"Ошибка при выполнении команды '{command}'");
                    return new HelpCommand();
                }
            }

            Console.WriteLine($"Неизвестная команда: '{command}'. Введите 'help' для справки.");
            return new HelpCommand();
        }

        private static ICommand ParseProfileCommand(string[] args)
        {
            bool logout = args.Any(a => a == "-o" || a == "--out");
            return new ProfileCommand(logout);
        }

        private static ICommand ParseAddCommand(string[] args)
        {
            bool isMultiline = args.Any(a => a == "-m" || a == "--multiline");

            if (isMultiline)
            {
                return new AddCommand("", true);
            }

            string text = string.Join(" ", args);
            text = text.Trim('"');

            return new AddCommand(text, false);
        }

        private static ICommand ParseViewCommand(string[] args)
        {
            bool showIndex = args.Any(a => a == "-i" || a == "--index");
            bool showStatus = args.Any(a => a == "-s" || a == "--status");
            bool showDate = args.Any(a => a == "-d" || a == "--update-date");
            bool showAll = args.Any(a => a == "-a" || a == "--all");

            if (showAll)
                return new ViewCommand(true, true, true);

            return new ViewCommand(showIndex, showStatus, showDate);
        }

        private static ICommand ParseSearchCommand(string[] args)
        {
            string? contains = null;
            string? startsWith = null;
            string? endsWith = null;
            DateTime? from = null;
            DateTime? to = null;
            TodoStatus? status = null;
            string? sort = null;
            bool desc = false;
            int? top = null;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--contains":
                        if (!TryGetValue(args, ref i, "--contains", out contains))
                            return new HelpCommand();
                        break;
                    case "--starts-with":
                        if (!TryGetValue(args, ref i, "--starts-with", out startsWith))
                            return new HelpCommand();
                        break;
                    case "--ends-with":
                        if (!TryGetValue(args, ref i, "--ends-with", out endsWith))
                            return new HelpCommand();
                        break;
                    case "--from":
                        if (!TryGetValue(args, ref i, "--from", out var fromText))
                            return new HelpCommand();

                        if (!DateTime.TryParse(fromText, out var fromDate))
                        {
                            Console.WriteLine("Некорректная дата. Используйте формат yyyy-MM-dd.");
                            return new HelpCommand();
                        }

                        from = fromDate;
                        break;
                    case "--to":
                        if (!TryGetValue(args, ref i, "--to", out var toText))
                            return new HelpCommand();

                        if (!DateTime.TryParse(toText, out var toDate))
                        {
                            Console.WriteLine("Некорректная дата. Используйте формат yyyy-MM-dd.");
                            return new HelpCommand();
                        }

                        to = toDate;
                        break;
                    case "--status":
                        if (!TryGetValue(args, ref i, "--status", out var statusText))
                            return new HelpCommand();

                        if (!TryParseStatus(statusText, out var parsedStatus))
                        {
                            Console.WriteLine("Неизвестный статус. Доступны: notstarted, in-progress, completed, postponed, failed");
                            return new HelpCommand();
                        }

                        status = parsedStatus;
                        break;
                    case "--sort":
                        if (!TryGetValue(args, ref i, "--sort", out sort))
                            return new HelpCommand();

                        sort = sort.ToLower();
                        if (sort != "text" && sort != "date")
                        {
                            Console.WriteLine("Некорректная сортировка. Используйте: --sort text или --sort date.");
                            return new HelpCommand();
                        }
                        break;
                    case "--desc":
                        desc = true;
                        break;
                    case "--top":
                        if (!TryGetValue(args, ref i, "--top", out var topText))
                            return new HelpCommand();

                        if (!int.TryParse(topText, out var parsedTop) || parsedTop <= 0)
                        {
                            Console.WriteLine("Параметр --top должен быть положительным числом.");
                            return new HelpCommand();
                        }

                        top = parsedTop;
                        break;
                    default:
                        Console.WriteLine($"Неизвестный флаг search: {args[i]}");
                        return new HelpCommand();
                }
            }

            return new SearchCommand(contains, startsWith, endsWith, from, to, status, sort, desc, top);
        }

        private static bool TryGetValue(string[] args, ref int index, string flag, out string value)
        {
            value = string.Empty;
            if (index + 1 >= args.Length || args[index + 1].StartsWith("--"))
            {
                Console.WriteLine($"Для {flag} нужно указать значение.");
                return false;
            }

            value = args[++index];
            return true;
        }

        private static bool TryParseStatus(string statusText, out TodoStatus status)
        {
            var normalizedStatus = statusText.Replace("-", "");
            return Enum.TryParse(normalizedStatus, ignoreCase: true, out status);
        }

        private static ICommand ParseReadCommand(string[] args)
        {
            if (args.Length > 0 && int.TryParse(args[0], out int index))
            {
                return new ReadCommand(index);
            }

            Console.WriteLine("Используйте: read <индекс>");
            return new HelpCommand();
        }

        private static ICommand ParseStatusCommand(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Используйте: status <индекс> <статус>");
                return new HelpCommand();
            }

            if (!int.TryParse(args[0], out int index))
            {
                Console.WriteLine("Индекс должен быть числом.");
                return new HelpCommand();
            }

            string statusStr = args[1].ToLower();
            if (Enum.TryParse<TodoStatus>(statusStr, ignoreCase: true, out var status))
            {
                return new StatusCommand(index, status);
            }

            Console.WriteLine("Неизвестный статус. Доступные: NotStarted, InProgress, Completed, Postponed, Failed");
            return new HelpCommand();
        }

        private static ICommand ParseUpdateCommand(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Используйте: update <индекс> \"новый текст\"");
                return new HelpCommand();
            }

            if (!int.TryParse(args[0], out int index))
            {
                Console.WriteLine("Индекс должен быть числом.");
                return new HelpCommand();
            }

            string newText = string.Join(" ", args.Skip(1)).Trim('"');
            return new UpdateCommand(index, newText);
        }

        private static ICommand ParseDeleteCommand(string[] args)
        {
            if (args.Length == 0 || !int.TryParse(args[0], out int index))
            {
                Console.WriteLine("Используйте: delete <индекс>");
                return new HelpCommand();
            }

            return new DeleteCommand(index);
        }

        private static string[] SplitCommand(string input)
        {
            var result = new List<string>();
            var regex = new Regex(@"[^\s""]+|""([^""]*)""");
            var matches = regex.Matches(input);

            foreach (Match match in matches)
            {
                if (match.Groups[1].Success)
                {
                    result.Add(match.Groups[1].Value);
                }
                else
                {
                    result.Add(match.Value);
                }
            }

            return result.ToArray();
        }
    }
}
