using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Desktop.Services;
using TodoApp.Models;

namespace TodoApp.Desktop.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ProfileRepository _profileRepository;
        private readonly TodoRepository _todoRepository;
        private ViewModelBase _currentView;
        private Profile? _currentProfile;

        public MainViewModel()
        {
            using (var context = new AppDbContext())
            {
                context.Database.Migrate();
            }

            _profileRepository = new ProfileRepository();
            _todoRepository = new TodoRepository();
            _currentView = new LoginViewModel(this, _profileRepository);
            ShowLoginCommand = new RelayCommand(_ => ShowLogin());
            ShowRegisterCommand = new RelayCommand(_ => ShowRegister());
            LogoutCommand = new RelayCommand(_ => Logout(), _ => CurrentProfile != null);
        }

        public ViewModelBase CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public Profile? CurrentProfile
        {
            get => _currentProfile;
            private set
            {
                if (SetProperty(ref _currentProfile, value))
                {
                    OnPropertyChanged(nameof(IsAuthenticated));
                    OnPropertyChanged(nameof(ProfileTitle));
                    ((RelayCommand)LogoutCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsAuthenticated => CurrentProfile != null;
        public string ProfileTitle => CurrentProfile == null ? "Войдите или зарегистрируйтесь" : CurrentProfile.GetInfo();
        public ICommand ShowLoginCommand { get; }
        public ICommand ShowRegisterCommand { get; }
        public ICommand LogoutCommand { get; }

        public void ShowLogin()
        {
            CurrentView = new LoginViewModel(this, _profileRepository);
        }

        public void ShowRegister()
        {
            CurrentView = new RegisterViewModel(this, _profileRepository);
        }

        public void Login(Profile profile)
        {
            CurrentProfile = profile;
            CurrentView = new TodoListViewModel(this, _todoRepository, profile);
        }

        public void Logout()
        {
            CurrentProfile = null;
            ShowLogin();
        }

        public void ShowTodoList()
        {
            if (CurrentProfile != null)
                CurrentView = new TodoListViewModel(this, _todoRepository, CurrentProfile);
        }
    }
}
