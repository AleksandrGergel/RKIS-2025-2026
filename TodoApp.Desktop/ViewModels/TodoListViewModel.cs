using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TodoApp.Data;
using TodoApp.Desktop.Services;
using TodoApp.Models;

namespace TodoApp.Desktop.ViewModels
{
    public class TodoListViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly TodoRepository _todoRepository;
        private readonly Profile _profile;
        private TodoItem? _selectedTodo;
        private string _searchText = string.Empty;
        private TodoStatus? _statusFilter;

        public TodoListViewModel(MainViewModel mainViewModel, TodoRepository todoRepository, Profile profile)
        {
            _mainViewModel = mainViewModel;
            _todoRepository = todoRepository;
            _profile = profile;
            Todos = new ObservableCollection<TodoItem>();
            FilteredTodos = new ObservableCollection<TodoItem>();
            Statuses = Enum.GetValues<TodoStatus>();
            AddCommand = new RelayCommand(_ => _mainViewModel.CurrentView = new AddTaskViewModel(_mainViewModel, _todoRepository, _profile));
            EditCommand = new RelayCommand(_ => EditSelected(), _ => SelectedTodo != null);
            DeleteCommand = new RelayCommand(_ => DeleteSelected(), _ => SelectedTodo != null);
            ClearFilterCommand = new RelayCommand(_ => ClearFilter());
            RefreshCommand = new RelayCommand(_ => LoadTodos());
            LoadTodos();
        }

        public ObservableCollection<TodoItem> Todos { get; }
        public ObservableCollection<TodoItem> FilteredTodos { get; }
        public TodoStatus[] Statuses { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand RefreshCommand { get; }

        public TodoItem? SelectedTodo
        {
            get => _selectedTodo;
            set
            {
                if (SetProperty(ref _selectedTodo, value))
                {
                    ((RelayCommand)EditCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DeleteCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    ApplyFilter();
            }
        }

        public TodoStatus? StatusFilter
        {
            get => _statusFilter;
            set
            {
                if (SetProperty(ref _statusFilter, value))
                    ApplyFilter();
            }
        }

        private void LoadTodos()
        {
            Todos.Clear();
            foreach (var todo in _todoRepository.GetAll(_profile.Id))
            {
                Todos.Add(todo);
            }

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var query = Todos.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                query = query.Where(t => t.Text.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            if (StatusFilter.HasValue)
            {
                query = query.Where(t => t.Status == StatusFilter.Value);
            }

            FilteredTodos.Clear();
            foreach (var todo in query.OrderBy(t => t.Id))
            {
                FilteredTodos.Add(todo);
            }
        }

        private void EditSelected()
        {
            if (SelectedTodo != null)
                _mainViewModel.CurrentView = new EditTaskViewModel(_mainViewModel, _todoRepository, _profile, SelectedTodo);
        }

        private void DeleteSelected()
        {
            if (SelectedTodo == null)
                return;

            _todoRepository.Delete(SelectedTodo.Id);
            Todos.Remove(SelectedTodo);
            SelectedTodo = null;
            ApplyFilter();
        }

        private void ClearFilter()
        {
            SearchText = string.Empty;
            StatusFilter = null;
            ApplyFilter();
        }
    }
}
