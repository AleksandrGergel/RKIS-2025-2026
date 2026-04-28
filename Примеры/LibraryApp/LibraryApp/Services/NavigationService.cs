using CommunityToolkit.Mvvm.ComponentModel;
using LibraryApp.ViewModels;

namespace LibraryApp.Services;

public class NavigationService
{
	private readonly MainWindowViewModel _mainVm;

	public NavigationService(MainWindowViewModel mainVm)
		=> _mainVm = mainVm;

	public void NavigateTo<TViewModel>() where TViewModel : ObservableObject, new()
		=> _mainVm.CurrentViewModel = new TViewModel();

	public void NavigateTo(ObservableObject viewModel)
		=> _mainVm.CurrentViewModel = viewModel;
}