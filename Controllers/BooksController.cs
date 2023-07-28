using FirstWeb.DTOS;
using FirstWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SQLite;

namespace FirstWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BooksController : ControllerBase
{
    private const string connection = "Data source = mydata.db; Version =3";

    private readonly List<string> createTableQueries = new ()
    {
        "CREATE TABLE IF NOT EXISTS Books (Id INTEGER PRIMARY KEY, Name TEXT, Price REAL, AuthorName TEXT, WriterId INTEGER)",
        "CREATE TABLE IF NOT EXISTS Genres(Id INT PRIMARY KEY,Name NVARCHAR(100) NOT NULL);",
        "CREATE TABLE IF NOT EXISTS BookGenres(BookId INT NOT NULL,GenreId INT NOT NULL,PRIMARY KEY (BookId, GenreId),FOREIGN KEY (BookId) " +
        "REFERENCES Books (Id),FOREIGN KEY (GenreId) REFERENCES Genres (Id));"
    };
    
    private void CreateTables()
    {
        using SQLiteConnection conn = new SQLiteConnection(connection);

        conn.Open();

        foreach (var query in createTableQueries)
        {
            using SQLiteCommand command = new SQLiteCommand(query, conn);
            command.ExecuteNonQuery();
        }
    }


    [HttpPost]
    [ProducesResponseType(typeof(Book),200)]
    public IActionResult CreateBook(CreateBookDTO bookModel)
    {
        CreateTables();

        using SQLiteConnection conn = new SQLiteConnection(connection);
        conn.Open();

        string insertQuery = "INSERT INTO Books (Name, Price, AuthorName,WriterId) VALUES (@name, @price, @author,@writerId)";

        using (SQLiteCommand command = new SQLiteCommand(insertQuery, conn))
        {
            command.Parameters.AddWithValue("@name", $"{bookModel.Name}");
            command.Parameters.AddWithValue("@price", bookModel.Price);
            command.Parameters.AddWithValue("@author", $"{bookModel.AuthorName}");
            command.Parameters.AddWithValue("@writerId",$"{bookModel.WriterId}");
            command.ExecuteNonQuery();
        }

        return Ok();
    }

    [HttpGet]
    public IActionResult GetBooks()
    {
        CreateTables();
        return Ok();
    }
    
    /// <summary>
    /// Will return a book or not found result.
    /// </summary>
    /// <param name="id">This is book's id.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Book),200)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetBook(int id)
    {
        CreateTables();
        return NotFound();
    }

    [HttpPut("{id}")]
    public IActionResult UpdateBook(int id,CreateBookDTO updateModel)
    {
        CreateTables();
        
        return Ok();
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteBook(int id)
    {
        CreateTables();

        return Ok(true);
    }
}