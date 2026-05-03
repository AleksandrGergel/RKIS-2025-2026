using System;
using System.Windows.Input;
using TodoApp.Data;
using TodoApp.Desktop.Services;
using TodoApp.Models;

namespace TodoApp.Desktop.ViewModels
{
    public class AddTaskViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly TodoRepository _todoRepository;
        private readonly Profile _profile;
        private string _text = string.Empty;
        private TodoStatus _status = TodoStatus.NotStarted;
        private string _message = string.Empty;

        public AddTaskViewModel(MainViewModel mainViewModel, TodoRepository todoRepository, Profile profile)
        {
            _mainViewModel = mainViewModel;
            _todoRepository = todoRepository;
            _profile = profile;
            Statuses = Enum.GetValues<TodoStatus>();
            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => _mainViewModel.ShowTodoList());
        }

        public string Text { get => _text; set => SetProperty(ref _text, value); }
        public TodoStatus Status { get => _status; set => SetProperty(ref _status, value); }
        public string Message { get => _message; set => SetProperty(ref _message, value); }
        public TodoStatus[] Statuses { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(Text))
            {
                Message = "Введите текст задачи.";
                return;
            }

            var item = new TodoItem(Text.Trim())
            {
                Status = Status,
                ProfileId = _profile.Id
            };
            _todoRepository.Add(item);
            _mainViewModel.ShowTodoList();
        }
    }
}
