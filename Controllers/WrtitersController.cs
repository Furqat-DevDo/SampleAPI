using FirstWeb.DTOS;
using FirstWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SQLite;

namespace FirstWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class WrtitersController : ControllerBase
{
    private static int count = 1;
    private static List<Writer> Writers = new();
    private const string connection = "Data source = mydata.db; Version =3";

    private readonly List<string> createTableQueries = new()
    {
        "CREATE TABLE IF NOT EXISTS Writers (Id INTEGER PRIMARY KEY AUTOINCREMENT, FullName TEXT, BirthDate DATETIME)",
        "CREATE TABLE IF NOT EXISTS WriterBooks (WriterId INT NOT NULL, BookId INT NOT NULL, PRIMARY KEY (WriterId, BookId), FOREIGN KEY (WriterId) REFERENCES Writers (Id), FOREIGN KEY (BookId) REFERENCES Books (Id);"
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

    /// <summary>
    /// Siz yangi yozuvchi modelini yaratishiz mumkin!
    /// </summary>
    /// <param name="writerModel"></param>
    /// <remarks>
    /// POST/TODO
    ///      {
    ///          "fullName": "string",
    ///          "birthDate": "2023-07-27T13:25:33.673Z",
    ///          "books": [
    ///            {
    ///              "id": 0,
    ///              "name": "string",
    ///              "price": 0,
    ///              "authorName": "string",
    ///              "writerId": 0,
    ///              "genres": [0]
    ///            }
    ///                 ]
    ///     }       
    /// </remarks>
    /// <response code="200">Returns the newly created Writer</response>

    [HttpPost]
    [ProducesResponseType(typeof(Writer),200)]
    public IActionResult CreateWriter(CreateWriterModel writerModel)
    {
        using SQLiteConnection conn = new SQLiteConnection(connection);
        conn.Open();

        string insertQuery = "INSERT INTO Writers (FullName, BirthDate) VALUES (@fullname, @birthdate)";

        using (SQLiteCommand command = new SQLiteCommand(insertQuery, conn))
        {
            command.Parameters.AddWithValue("@fullname", $"{writerModel.FullName}");
            command.Parameters.AddWithValue("@birthdate", writerModel.BirthDate);

            command.ExecuteNonQuery();
        }

        return Ok();

    }

    [HttpGet]
    public IActionResult GetWriters()
    {
        return Ok(Writers);
    }

    /// <summary>
    /// Will return a writer or not found result.
    /// </summary>
    /// <param name="id">This is writer's id.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof (Writer),200)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetWriter(int id)
    {
        var writer = Writers.FirstOrDefault(x => x.Id == id);
        if (writer == null)  return NotFound();

        return Ok(writer);
    }

    [HttpPut("{id}")]
    public IActionResult PutWriter(int id, CreateWriterModel writerModel)
    {
        var writer = Writers.FirstOrDefault(y => y.Id == id);
        if (writer == null) return NotFound();

        writer.FullName = writerModel.FullName;
        writer.BirthDate = writerModel.BirthDate;
        writer.Books = writerModel.Books;

        return Ok(writer);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteWriter(int id)
    {
        var writer = Writers.FirstOrDefault(x=>x.Id == id);
        if (writer == null) return NotFound(false);
        Writers.Remove(writer);

        return Ok(true);
    }
}