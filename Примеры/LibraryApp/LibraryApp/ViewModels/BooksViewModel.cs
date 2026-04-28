using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibraryApp.Models;
using LibraryApp.Services;
using System.Collections.ObjectModel;

namespace LibraryApp.ViewModels;

public partial class BooksViewModel : ObservableObject
{
	private readonly BookService _bookService;
	private readonly NavigationService _navigation;

	// ObservableCollection уведомляет UI при добавлении/удалении элементов
	public ObservableCollection<Book> Books { get; } = new();

	// При смене выбранной книги пересчитывается IsBookSelected,
	// и обновляется доступность команд Edit и Delete
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(IsBookSelected))]
	[NotifyCanExecuteChangedFor(nameof(EditCommand))]
	[NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
	private Book? _selectedBook;

	public bool IsBookSelected => SelectedBook != null;

	public BooksViewModel(NavigationService navigation)
	{
		_bookService = BookService.Instance;
		_navigation  = navigation;
		Load();
	}

	private void Load()
	{
		Books.Clear();
		foreach (var book in _bookService.GetAll())
			Books.Add(book);
	}

	[RelayCommand]
	private void Add()
	{
		// Передаём callback: после сохранения книга добавится в список
		_navigation.NavigateTo(new BookEditViewModel(
			null,
			onSaved: savedBook =>
			{
				Books.Add(savedBook);
				_navigation.NavigateTo(this);
			},
			onCancel: () => _navigation.NavigateTo(this)));
	}

	[RelayCommand(CanExecute = nameof(IsBookSelected))]
	private void Edit()
	{
		if (SelectedBook is null) return;

		// Передаём редактируемую книгу и callback на обновление списка
		_navigation.NavigateTo(new BookEditViewModel(
			SelectedBook,
			onSaved: savedBook =>
			{
				var index = Books.IndexOf(Books.First(b => b.Id == savedBook.Id));
				Books[index] = savedBook;
				_navigation.NavigateTo(this);
			},
			onCancel: () => _navigation.NavigateTo(this)));
	}

	[RelayCommand(CanExecute = nameof(IsBookSelected))]
	private void Delete()
	{
		if (SelectedBook is null) return;
		_bookService.Delete(SelectedBook.Id);
		Books.Remove(SelectedBook);
	}
}