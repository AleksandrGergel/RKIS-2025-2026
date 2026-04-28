using LibraryApp.Models;

namespace LibraryApp.Services;

public class AuthorService
{
	//Instance всегда будет возвращать один и тот же экземпляр AuthorService
	private static readonly AuthorService _instance = new();
	public static AuthorService Instance => _instance;

	private AuthorService() { }

	private readonly List<Author> _authors = new()
	{
		new Author { Id = 1, FirstName = "Михаил",  LastName = "Булгаков",    Country = "Россия" },
		new Author { Id = 2, FirstName = "Фёдор",   LastName = "Достоевский", Country = "Россия" },
		new Author { Id = 3, FirstName = "Лев",     LastName = "Толстой",     Country = "Россия" },
	};

	private int _nextId = 4;

	public List<Author> GetAll() => _authors.ToList();

	public void Add(Author author)
	{
		author.Id = _nextId++;
		_authors.Add(author);
	}

	public void Update(Author author)
	{
		var index = _authors.FindIndex(a => a.Id == author.Id);
		if (index >= 0)
			_authors[index] = author;
	}

	public void Delete(int id)
	{
		var author = _authors.FirstOrDefault(a => a.Id == id);
		if (author != null)
			_authors.Remove(author);
	}
}