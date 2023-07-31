namespace FirstWeb.Models;

public class BookModel
{
    public long Id { get; set; }
    public string Name { get; set; }
    public double Price { get; set; }
    public string AuthorName { get; set; }
    public long WriterId { get; set; }
    public List<GenreModel> Genres { get; set; } = new();
}
