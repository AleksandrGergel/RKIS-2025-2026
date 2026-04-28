using LibraryApp.Models;

namespace LibraryApp.Services;

public class BookService
{
	//Instance всегда будет возвращать один и тот же экземпляр BookService
	private static readonly BookService _instance = new();
	public static BookService Instance => _instance;

	private BookService() { }

	private readonly List<Book> _books = new()
	{
		new Book { Id = 1, Title = "Мастер и Маргарита", Author = "Булгаков", Year = 1967, Genre = "Роман", IsAvailable = true },
		new Book { Id = 2, Title = "Преступление и наказание", Author = "Достоевский", Year = 1866, Genre = "Роман", IsAvailable = true },
		new Book { Id = 3, Title = "Война и мир", Author = "Толстой", Year = 1869, Genre = "Роман-эпопея", IsAvailable = false },
	};

	private int _nextId = 4;

	public List<Book> GetAll() => _books.ToList();
	public Book? GetById(int id) => _books.FirstOrDefault(b => b.Id == id);

	public void Add(Book book)
	{
		book.Id = _nextId++;
		_books.Add(book);
	}

	public void Update(Book book)
	{
		var index = _books.FindIndex(b => b.Id == book.Id);
		if (index >= 0)
			_books[index] = book;
	}

	public void Delete(int id)
	{
		var book = _books.FirstOrDefault(b => b.Id == id);
		if (book != null)
			_books.Remove(book);
	}
}