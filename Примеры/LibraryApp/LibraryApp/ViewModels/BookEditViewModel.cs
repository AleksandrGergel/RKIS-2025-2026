using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibraryApp.Models;
using LibraryApp.Services;

namespace LibraryApp.ViewModels;

public partial class BookEditViewModel : ObservableObject
{
	private readonly BookService _bookService;
	private readonly Book? _editingBook; // null = режим добавления
	private readonly Action<Book>? _onSaved;
	private readonly Action? _onCancel;

	// [NotifyCanExecuteChangedFor] сообщает команде Save,
	// что её CanExecute нужно пересчитать при изменении этого поля
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(CanSave))]
	[NotifyCanExecuteChangedFor(nameof(SaveCommand))]
	private string _title = "";

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(CanSave))]
	[NotifyCanExecuteChangedFor(nameof(SaveCommand))]
	private string _author = "";

	[ObservableProperty]
	private int _year = DateTime.Now.Year;

	[ObservableProperty]
	private string _genre = "";

	[ObservableProperty]
	private bool _isAvailable = true;

	public string PageTitle => _editingBook == null ? "Добавить книгу" : "Редактировать книгу";

	public bool CanSave =>
		!string.IsNullOrWhiteSpace(Title) &&
		!string.IsNullOrWhiteSpace(Author);

	public BookEditViewModel(
		Book? book,
		Action<Book>? onSaved = null,
		Action? onCancel = null)
	{
		_bookService = BookService.Instance;
		_editingBook = book;
		_onSaved     = onSaved;
		_onCancel    = onCancel;

		// Заполняем поля, если редактируем существующую книгу
		if (book != null)
		{
			Title       = book.Title;
			Author      = book.Author;
			Year        = book.Year;
			Genre       = book.Genre;
			IsAvailable = book.IsAvailable;
		}
	}

	[RelayCommand(CanExecute = nameof(CanSave))]
	private void Save()
	{
		var book = new Book
		{
			Id          = _editingBook?.Id ?? 0,
			Title       = Title,
			Author      = Author,
			Year        = Year,
			Genre       = Genre,
			IsAvailable = IsAvailable
		};

		if (_editingBook == null)
			_bookService.Add(book);
		else
			_bookService.Update(book);

		_onSaved?.Invoke(book);
	}

	[RelayCommand]
	private void Cancel() => _onCancel?.Invoke();
}