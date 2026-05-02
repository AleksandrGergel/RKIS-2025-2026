using System;
using System.Linq;
using TodoApp.Exceptions;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp.Commands
{
    public class SyncCommand : ICommand
    {
        private readonly bool _pull;
        private readonly bool _push;

        public SyncCommand(bool pull, bool push)
        {
            _pull = pull;
            _push = push;
        }

        public void Execute()
        {
            var apiStorage = new ApiDataStorage("http://localhost:5000/");
            if (!apiStorage.IsAvailable())
            {
                Console.WriteLine("Ошибка: сервер недоступен.");
                return;
            }

            var localStorage = new FileManager("data");

            if (_pull)
            {
                Pull(apiStorage, localStorage);
            }

            if (_push)
            {
                Push(apiStorage);
            }
        }

        private void Push(ApiDataStorage apiStorage)
        {
            apiStorage.SaveProfiles(AppInfo.Profiles);

            var currentProfile = AppInfo.CurrentProfile
                ?? throw new AuthenticationException("Пользователь не авторизован.");

            var todos = AppInfo.RequireCurrentTodoList();
            apiStorage.SaveTodos(currentProfile.Id, todos.GetAll());

            Console.WriteLine("Данные отправлены на сервер.");
        }

        private void Pull(ApiDataStorage apiStorage, FileManager localStorage)
        {
            var profiles = apiStorage.LoadProfiles().ToList();
            AppInfo.Profiles = profiles;
            localStorage.SaveProfiles(AppInfo.Profiles);

            if (AppInfo.CurrentProfile != null)
            {
                var actualProfile = AppInfo.Profiles.FirstOrDefault(p => p.Id == AppInfo.CurrentProfile.Id);
                if (actualProfile == null)
                {
                    AppInfo.CurrentProfile = null;
                    AppInfo.UserTodos.Clear();
                    AppInfo.ClearUndoRedo();
                    Console.WriteLine("Профиль не найден на сервере. Выполните вход заново.");
                    return;
                }

                AppInfo.CurrentProfile = actualProfile;
                var loadedTodos = apiStorage.LoadTodos(actualProfile.Id).ToList();
                localStorage.SaveTodos(actualProfile.Id, loadedTodos);

                if (!AppInfo.UserTodos.TryGetValue(actualProfile.Id, out var todoList))
                {
                    todoList = new TodoList();
                    AppInfo.UserTodos[actualProfile.Id] = todoList;
                }

                todoList.GetAll().Clear();
                foreach (var item in loadedTodos)
                {
                    todoList.Add(item);
                }
            }

            Console.WriteLine("Данные получены с сервера.");
        }
    }
}
