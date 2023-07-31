namespace FirstWeb.DTOS;

public class CreateBookModel
{
    public string Name { get; set; }
    public float Price { get; set; }
    public string AuthorName { get; set; }
    public int WriterId { get; set; }
    public List<CreateGenreModel> Genres { get; set; }
}