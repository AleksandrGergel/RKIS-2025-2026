using System;
using System.Windows.Input;
using TodoApp.Data;
using TodoApp.Desktop.Services;
using TodoApp.Models;

namespace TodoApp.Desktop.ViewModels
{
    public class RegisterViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly ProfileRepository _profileRepository;
        private string _login = string.Empty;
        private string _password = string.Empty;
        private string _firstName = string.Empty;
        private string _lastName = string.Empty;
        private string _birthYear = string.Empty;
        private string _message = string.Empty;

        public RegisterViewModel(MainViewModel mainViewModel, ProfileRepository profileRepository)
        {
            _mainViewModel = mainViewModel;
            _profileRepository = profileRepository;
            RegisterCommand = new RelayCommand(_ => Register());
            BackCommand = new RelayCommand(_ => _mainViewModel.ShowLogin());
        }

        public string Login { get => _login; set => SetProperty(ref _login, value); }
        public string Password { get => _password; set => SetProperty(ref _password, value); }
        public string FirstName { get => _firstName; set => SetProperty(ref _firstName, value); }
        public string LastName { get => _lastName; set => SetProperty(ref _lastName, value); }
        public string BirthYear { get => _birthYear; set => SetProperty(ref _birthYear, value); }
        public string Message { get => _message; set => SetProperty(ref _message, value); }
        public ICommand RegisterCommand { get; }
        public ICommand BackCommand { get; }

        private void Register()
        {
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
            {
                Message = "Заполните все поля.";
                return;
            }

            if (!int.TryParse(BirthYear, out int year) || year < 1900 || year > DateTime.Now.Year)
            {
                Message = "Введите корректный год рождения.";
                return;
            }

            if (_profileRepository.LoginExists(Login.Trim()))
            {
                Message = "Этот логин уже занят.";
                return;
            }

            var profile = new Profile(Login.Trim(), Password, FirstName.Trim(), LastName.Trim(), year);
            _profileRepository.Add(profile);
            _mainViewModel.Login(profile);
        }
    }
}
