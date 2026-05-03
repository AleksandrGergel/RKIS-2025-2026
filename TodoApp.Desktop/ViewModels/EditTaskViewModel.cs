using System;
using System.Windows.Input;
using TodoApp.Data;
using TodoApp.Desktop.Services;
using TodoApp.Models;

namespace TodoApp.Desktop.ViewModels
{
    public class EditTaskViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly TodoRepository _todoRepository;
        private readonly Profile _profile;
        private readonly TodoItem _todo;
        private string _text;
        private TodoStatus _status;
        private string _message = string.Empty;

        public EditTaskViewModel(MainViewModel mainViewModel, TodoRepository todoRepository, Profile profile, TodoItem todo)
        {
            _mainViewModel = mainViewModel;
            _todoRepository = todoRepository;
            _profile = profile;
            _todo = todo;
            _text = todo.Text;
            _status = todo.Status;
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

            _todo.Text = Text.Trim();
            _todo.Status = Status;
            _todo.ProfileId = _profile.Id;
            _todo.Profile = null;
            _todo.LastUpdate = DateTime.Now;
            _todoRepository.Update(_todo);
            _mainViewModel.ShowTodoList();
        }
    }
}
