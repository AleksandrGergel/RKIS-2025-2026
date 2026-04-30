using System;
using System.Collections.Generic;
using System.Linq;
using TodoApp.Commands;
using TodoApp.Exceptions;
using TodoApp.Models;

namespace TodoApp.Services
{
    public static class AppInfo
    {
        public static List<Profile> Profiles { get; set; } = new();
        public static Profile? CurrentProfile { get; set; }
        public static Dictionary<Guid, TodoList> UserTodos { get; set; } = new();
        public static Stack<IUndoableCommand> UndoStack { get; set; } = new();
        public static Stack<IUndoableCommand> RedoStack { get; set; } = new();

        public static TodoList? GetCurrentTodoList()
        {
            if (CurrentProfile != null && UserTodos.ContainsKey(CurrentProfile.Id))
            {
                return UserTodos[CurrentProfile.Id];
            }
            return null;
        }

        public static TodoList RequireCurrentTodoList()
        {
            var todos = GetCurrentTodoList();
            if (todos == null)
            {
                throw new AuthenticationException("Пользователь не авторизован.");
            }

            return todos;
        }

        public static void ClearUndoRedo()
        {
            UndoStack.Clear();
            RedoStack.Clear();
        }
    }
}
