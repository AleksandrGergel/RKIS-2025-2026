using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibraryApp.Models;
using LibraryApp.Services;

namespace LibraryApp.ViewModels;

public partial class AuthorEditViewModel : ObservableObject
{
	private readonly AuthorService _authorService;
	private readonly Author? _editingAuthor;
	private readonly Action<Author>? _onSaved;
	private readonly Action? _onCancel;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(CanSave))]
	[NotifyCanExecuteChangedFor(nameof(SaveCommand))]
	private string _firstName = "";

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(CanSave))]
	[NotifyCanExecuteChangedFor(nameof(SaveCommand))]
	private string _lastName = "";

	[ObservableProperty]
	private string _country = "";

	public string PageTitle => _editingAuthor == null ? "Добавить автора" : "Редактировать автора";

	public bool CanSave =>
		!string.IsNullOrWhiteSpace(FirstName) &&
		!string.IsNullOrWhiteSpace(LastName);

	public AuthorEditViewModel(
		Author? author,
		Action<Author>? onSaved = null,
		Action? onCancel = null)
	{
		_authorService = AuthorService.Instance;
		_editingAuthor = author;
		_onSaved       = onSaved;
		_onCancel      = onCancel;

		if (author != null)
		{
			FirstName = author.FirstName;
			LastName  = author.LastName;
			Country   = author.Country;
		}
	}

	[RelayCommand(CanExecute = nameof(CanSave))]
	private void Save()
	{
		var author = new Author
		{
			Id        = _editingAuthor?.Id ?? 0,
			FirstName = FirstName,
			LastName  = LastName,
			Country   = Country
		};

		if (_editingAuthor == null)
			_authorService.Add(author);
		else
			_authorService.Update(author);

		_onSaved?.Invoke(author);
	}

	[RelayCommand]
	private void Cancel() => _onCancel?.Invoke();
}