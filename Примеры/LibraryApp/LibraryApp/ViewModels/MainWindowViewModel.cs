using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibraryApp.Services;

namespace LibraryApp.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
	private NavigationService? _navigation;

	// Текущий экран — ContentControl привязан к этому свойству.
	// При его смене WPF автоматически подбирает нужный UserControl через DataTemplate.
	[ObservableProperty]
	private ObservableObject? _currentViewModel;

	public void Initialize(NavigationService navigation)
		=> _navigation = navigation;

	[RelayCommand]
	private void NavigateToBooks()
		=> Navigation.NavigateTo(new BooksViewModel(Navigation));

	[RelayCommand]
	private void NavigateToAuthors()
		=> Navigation.NavigateTo(new AuthorsViewModel(Navigation));
	private NavigationService Navigation
		=> _navigation ?? throw new InvalidOperationException("NavigationService не инициализирован.");
}