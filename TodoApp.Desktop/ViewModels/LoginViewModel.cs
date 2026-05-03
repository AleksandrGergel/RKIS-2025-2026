using System.Windows.Input;
using TodoApp.Data;
using TodoApp.Desktop.Services;

namespace TodoApp.Desktop.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly ProfileRepository _profileRepository;
        private string _login = string.Empty;
        private string _password = string.Empty;
        private string _message = string.Empty;

        public LoginViewModel(MainViewModel mainViewModel, ProfileRepository profileRepository)
        {
            _mainViewModel = mainViewModel;
            _profileRepository = profileRepository;
            LoginCommand = new RelayCommand(_ => LoginProfile());
            RegisterCommand = new RelayCommand(_ => _mainViewModel.ShowRegister());
        }

        public string Login
        {
            get => _login;
            set => SetProperty(ref _login, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }

        private void LoginProfile()
        {
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
            {
                Message = "Введите логин и пароль.";
                return;
            }

            var profile = _profileRepository.GetByCredentials(Login.Trim(), Password);
            if (profile == null)
            {
                Message = "Неверный логин или пароль.";
                return;
            }

            _mainViewModel.Login(profile);
        }
    }
}
