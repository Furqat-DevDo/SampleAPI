namespace FirstWeb.Models;

public class Book
{
    public long Id { get; set; }
    public string Name { get; set; }
    public float Price { get; set; }
    public string AuthorName { get; set; }
    public long WriterId { get; set; }
    public List<Genre> Genres { get; set; } = new();
}