using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibraryApp.Models;
using LibraryApp.Services;
using System.Collections.ObjectModel;

namespace LibraryApp.ViewModels;

public partial class AuthorsViewModel : ObservableObject
{
	private readonly AuthorService _authorService;
	private readonly NavigationService _navigation;

	public ObservableCollection<Author> Authors { get; } = new();

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(IsAuthorSelected))]
	[NotifyCanExecuteChangedFor(nameof(EditCommand))]
	[NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
	private Author? _selectedAuthor;

	public bool IsAuthorSelected => SelectedAuthor != null;

	public AuthorsViewModel(NavigationService navigation)
	{
		_authorService = AuthorService.Instance;
		_navigation = navigation;
		Load();
	}

	private void Load()
	{
		Authors.Clear();
		foreach (var author in _authorService.GetAll())
			Authors.Add(author);
	}

	[RelayCommand]
	private void Add()
	{
		_navigation.NavigateTo(new AuthorEditViewModel(
			null,
			onSaved: saved =>
			{
				Authors.Add(saved);
				_navigation.NavigateTo(this);
			},
			onCancel: () => _navigation.NavigateTo(this)));
	}

	[RelayCommand(CanExecute = nameof(IsAuthorSelected))]
	private void Edit()
	{
		if (SelectedAuthor is null) return;

		_navigation.NavigateTo(new AuthorEditViewModel(
			SelectedAuthor,
			//callbacks
			onSaved: saved =>
			{
				var index = Authors.IndexOf(Authors.First(a => a.Id == saved.Id));
				Authors[index] = saved;
				_navigation.NavigateTo(this);
			},
			onCancel: () => _navigation.NavigateTo(this)));
	}

	[RelayCommand(CanExecute = nameof(IsAuthorSelected))]
	private void Delete()
	{
		if (SelectedAuthor is null) return;
		_authorService.Delete(SelectedAuthor.Id);
		Authors.Remove(SelectedAuthor);
	}
}