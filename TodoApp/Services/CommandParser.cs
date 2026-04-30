using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TodoApp.Commands;
using TodoApp.Exceptions;
using TodoApp.Models;

namespace TodoApp.Services
{
    public static class CommandParser
    {
        private static Dictionary<string, Func<string, ICommand>> _commandHandlers = new();
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
                ["load"] = args => ParseLoadCommand(SplitCommand(args)),
                ["undo"] = args => new UndoCommand(),
                ["redo"] = args => new RedoCommand(),
            };
        }

        public static ICommand Parse(string inputString)
        {
            if (string.IsNullOrWhiteSpace(inputString))
            {
                throw new InvalidCommandException("Команда не может быть пустой.");
            }

            var trimmedInput = inputString.Trim();
            var commandMatch = Regex.Match(trimmedInput, @"^\S+");
            if (!commandMatch.Success)
            {
                throw new InvalidCommandException("Команда не может быть пустой.");
            }

            string command = commandMatch.Value.ToLower();
            string args = trimmedInput.Substring(commandMatch.Length).TrimStart();

            if (!_commandHandlers.ContainsKey(command))
            {
                throw new InvalidCommandException($"Команда '{command}' не зарегистрирована.");
            }

            return _commandHandlers[command](args);
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
                return new AddCommand(string.Empty, true);
            }

            string text = string.Join(" ", args).Trim('"');
            return new AddCommand(text, false);
        }

        private static ICommand ParseViewCommand(string[] args)
        {
            bool showIndex = args.Any(a => a == "-i" || a == "--index");
            bool showStatus = args.Any(a => a == "-s" || a == "--status");
            bool showDate = args.Any(a => a == "-d" || a == "--update-date" || a == "--date");
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
                        contains = GetRequiredValue(args, ref i, "--contains");
                        break;
                    case "--starts-with":
                        startsWith = GetRequiredValue(args, ref i, "--starts-with");
                        break;
                    case "--ends-with":
                        endsWith = GetRequiredValue(args, ref i, "--ends-with");
                        break;
                    case "--from":
                        from = ParseDate(GetRequiredValue(args, ref i, "--from"), "--from");
                        break;
                    case "--to":
                        to = ParseDate(GetRequiredValue(args, ref i, "--to"), "--to");
                        break;
                    case "--status":
                        status = ParseStatus(GetRequiredValue(args, ref i, "--status"));
                        break;
                    case "--sort":
                        sort = GetRequiredValue(args, ref i, "--sort").ToLower();
                        if (sort != "text" && sort != "date")
                        {
                            throw new InvalidArgumentException("Сортировка должна быть text или date.");
                        }
                        break;
                    case "--desc":
                        desc = true;
                        break;
                    case "--top":
                        top = ParsePositiveInt(GetRequiredValue(args, ref i, "--top"), "--top");
                        break;
                    default:
                        throw new InvalidCommandException($"Неизвестный флаг search: {args[i]}");
                }
            }

            return new SearchCommand(contains, startsWith, endsWith, from, to, status, sort, desc, top);
        }

        private static ICommand ParseReadCommand(string[] args)
        {
            if (args.Length == 0)
            {
                throw new InvalidArgumentException("Используйте: read <индекс>");
            }

            return new ReadCommand(ParseIndex(args[0]));
        }

        private static ICommand ParseStatusCommand(string[] args)
        {
            if (args.Length < 2)
            {
                throw new InvalidArgumentException("Используйте: status <индекс> <статус>");
            }

            return new StatusCommand(ParseIndex(args[0]), ParseStatus(args[1]));
        }

        private static ICommand ParseUpdateCommand(string[] args)
        {
            if (args.Length < 2)
            {
                throw new InvalidArgumentException("Используйте: update <индекс> \"новый текст\"");
            }

            string newText = string.Join(" ", args.Skip(1)).Trim('"');
            return new UpdateCommand(ParseIndex(args[0]), newText);
        }

        private static ICommand ParseDeleteCommand(string[] args)
        {
            if (args.Length == 0)
            {
                throw new InvalidArgumentException("Используйте: delete <индекс>");
            }

            return new DeleteCommand(ParseIndex(args[0]));
        }

        private static ICommand ParseLoadCommand(string[] args)
        {
            if (args.Length < 2)
            {
                throw new InvalidArgumentException("Используйте: load <количество_скачиваний> <размер_скачиваний>");
            }

            int downloadsCount = ParsePositiveInt(args[0], "количество_скачиваний");
            int downloadSize = ParsePositiveInt(args[1], "размер_скачиваний");

            return new LoadCommand(downloadsCount, downloadSize);
        }

        private static int ParseIndex(string value)
        {
            if (!int.TryParse(value, out int index) || index < 0)
            {
                throw new InvalidArgumentException("Индекс должен быть неотрицательным числом.");
            }

            return index;
        }

        private static int ParsePositiveInt(string value, string argumentName)
        {
            if (!int.TryParse(value, out int number) || number <= 0)
            {
                throw new InvalidArgumentException($"Параметр {argumentName} должен быть положительным числом.");
            }

            return number;
        }

        private static DateTime ParseDate(string value, string argumentName)
        {
            if (!DateTime.TryParse(value, out var date))
            {
                throw new InvalidArgumentException($"Параметр {argumentName} должен быть датой в формате yyyy-MM-dd.");
            }

            return date;
        }

        private static TodoStatus ParseStatus(string value)
        {
            var normalizedStatus = value.Replace("-", "");
            if (Enum.TryParse<TodoStatus>(normalizedStatus, ignoreCase: true, out var status))
            {
                return status;
            }

            throw new InvalidArgumentException("Неизвестный статус. Доступны: notstarted, in-progress, completed, postponed, failed.");
        }

        private static string GetRequiredValue(string[] args, ref int index, string flag)
        {
            if (index + 1 >= args.Length || args[index + 1].StartsWith("--") || string.IsNullOrWhiteSpace(args[index + 1]))
            {
                throw new InvalidArgumentException($"Для {flag} нужно указать значение.");
            }

            return args[++index];
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
