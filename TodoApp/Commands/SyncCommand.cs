using System;
using System.Linq;
using TodoApp.Data;
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

            if (_pull)
            {
                Pull(apiStorage);
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

        private void Pull(ApiDataStorage apiStorage)
        {
            var profileRepository = new ProfileRepository();
            var todoRepository = new TodoRepository();

            var profiles = apiStorage.LoadProfiles().ToList();
            profileRepository.ReplaceAll(profiles);
            AppInfo.Profiles = profileRepository.GetAll();

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
                todoRepository.ReplaceForProfile(actualProfile.Id, loadedTodos);

                var todoList = new TodoList();
                foreach (var item in todoRepository.GetAll(actualProfile.Id))
                {
                    todoList.Add(item);
                }

                todoList.OnTodoAdded += item =>
                {
                    item.ProfileId = actualProfile.Id;
                    item.Profile = null;
                    todoRepository.Add(item);
                };
                todoList.OnTodoDeleted += item => todoRepository.Delete(item.Id);
                todoList.OnTodoUpdated += item =>
                {
                    item.ProfileId = actualProfile.Id;
                    item.Profile = null;
                    todoRepository.Update(item);
                };
                todoList.OnStatusChanged += item =>
                {
                    item.ProfileId = actualProfile.Id;
                    item.Profile = null;
                    todoRepository.Update(item);
                };

                AppInfo.UserTodos[actualProfile.Id] = todoList;
            }

            Console.WriteLine("Данные получены с сервера.");
        }
    }
}
