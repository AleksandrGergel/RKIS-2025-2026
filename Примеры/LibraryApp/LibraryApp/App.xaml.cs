using System.Windows;
using LibraryApp.Services;
using LibraryApp.ViewModels;
using LibraryApp.Views;

namespace LibraryApp;

public partial class App : Application
{
	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		// Создаём сервисы — хранят данные в памяти (списках)
		var bookService = BookService.Instance;
		var authorService = AuthorService.Instance;

		// Разрешаем циклическую зависимость:
		// NavigationService нужен MainWindowViewModel,
		// но и сам зависит от него — создаём оба, потом инициализируем
		var mainVm = new MainWindowViewModel();
		var navigation = new NavigationService(mainVm);

		mainVm.Initialize(navigation);

		// Стартовый экран — список книг
		navigation.NavigateTo(new BooksViewModel(navigation));

		new MainWindow { DataContext = mainVm }.Show();
	}
}