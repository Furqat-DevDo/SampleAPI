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
    /// <summary>
    /// Our Mock data source location.
    /// </summary>
    private const string connectionString = "Data source = mydata.db; Version =3";

    /// <summary>
    /// List of Queries for Create All Tables.
    /// </summary>
    private readonly List<string> createTableQueries = new ()
    {
        "CREATE TABLE IF NOT EXISTS Books (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT, Price REAL, AuthorName TEXT, WriterId INTEGER)",
        "CREATE TABLE IF NOT EXISTS Genres(Id INTEGER PRIMARY KEY AUTOINCREMENT,Name NVARCHAR(100) NOT NULL);",
        "CREATE TABLE IF NOT EXISTS BookGenres (BookId INTEGER NOT NULL,GenreId INTEGER NOT NULL,PRIMARY KEY (BookId, GenreId),FOREIGN KEY (BookId) " +
        "REFERENCES Books (Id),FOREIGN KEY (GenreId) REFERENCES Genres (Id));"
    };
    
    /// <summary>
    /// Will Ensure that tables are created.
    /// </summary>
    private void CreateTables()
    {
        using SQLiteConnection conn = new SQLiteConnection(connectionString);

        conn.Open();

        foreach (var query in createTableQueries)
        {
            using SQLiteCommand command = new SQLiteCommand(query, conn);
            command.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// Will Create a new book with it's genres.
    /// </summary>
    /// <param name="bookModel">Create book Model</param>
    [HttpPost]
    [ProducesResponseType(typeof(Book),200)]
    public IActionResult CreateBook(CreateBookDTO bookModel)
    {
        CreateTables();

        using SQLiteConnection conn = new SQLiteConnection(connectionString);
        conn.Open();

        string insertBookQuery = "INSERT INTO Books (Name, Price, AuthorName, WriterId) " +
                                 "VALUES (@name, @price, @author, @writerId)";
        using (SQLiteCommand bookCommand = new SQLiteCommand(insertBookQuery, conn))
        {
            bookCommand.Parameters.AddWithValue("@name", $"{bookModel.Name}");
            bookCommand.Parameters.AddWithValue("@price", bookModel.Price);
            bookCommand.Parameters.AddWithValue("@author", $"{bookModel.AuthorName}");
            bookCommand.Parameters.AddWithValue("@writerId", $"{bookModel.WriterId}");
            bookCommand.ExecuteNonQuery();
        }

        long bookId;
        using (SQLiteCommand getLastInsertRowIdCommand = new SQLiteCommand("SELECT last_insert_rowid();", conn))
        {
            bookId = Convert.ToInt64(getLastInsertRowIdCommand.ExecuteScalar());
        }

        
        foreach (var genre in bookModel.Genres)
        {

            string selectGenreQuery = "SELECT Id FROM Genres WHERE Name = @name";
            long genreId;
            using (SQLiteCommand selectGenreCommand = new SQLiteCommand(selectGenreQuery, conn))
            {
                selectGenreCommand.Parameters.AddWithValue("@name", genre.Name);
                var result = selectGenreCommand.ExecuteScalar();

                if (result != null)
                {
                    
                    genreId = (long)(result);
                }
                else
                {
                    
                    string insertGenreQuery = "INSERT INTO Genres (Name) VALUES (@name)";
                    using (SQLiteCommand genreCommand = new SQLiteCommand(insertGenreQuery, conn))
                    {
                        genreCommand.Parameters.AddWithValue("@name", genre.Name);
                        genreCommand.ExecuteNonQuery();
                    }

                    
                    using (SQLiteCommand getLastInsertRowIdCommand = new SQLiteCommand("SELECT last_insert_rowid();", conn))
                    {
                        genreId = (long)(getLastInsertRowIdCommand.ExecuteScalar());
                    }
                }
            }

            string insertBookGenreQuery = "INSERT INTO BookGenres (BookId, GenreId) VALUES (@bookId, @genreId)";
            using (SQLiteCommand bookGenreCommand = new SQLiteCommand(insertBookGenreQuery, conn))
            {
                bookGenreCommand.Parameters.AddWithValue("@bookId", bookId);
                bookGenreCommand.Parameters.AddWithValue("@genreId", genreId);
                bookGenreCommand.ExecuteNonQuery();
            }
        }

        var createdBook = GetBook(bookId);

        return CreatedAtAction(nameof(CreateBook), createdBook);
    }

    /// <summary>
    /// Will Get Genres from BooksGenre Table.
    /// </summary>
    /// <param name="bookId">bookId</param>
    /// <param name="conn">SQL Lite ConnectionString</param>
    private List<Genre> GetGenres(long bookId, SQLiteConnection conn)
    {
        var genres = new List<Genre>();

        using var command = new SQLiteCommand($"SELECT Genres.Id, Genres.Name AS GenreName " +
                                              $"FROM Genres INNER JOIN BookGenres ON Genres.Id = BookGenres.GenreId " +
                                              $"WHERE BookGenres.BookId = @bookId;", conn);
        command.Parameters.AddWithValue("@bookId", bookId);

        using var reader = command.ExecuteReader();

        if (reader.HasRows)
        {
            while (reader.Read())
            {
                genres.Add(new Genre
                {
                    Id = Convert.ToInt64(reader["Id"]),
                    Name = (string)reader["GenreName"],
                });
            }
        }

        return genres;
    }

    /// <summary>
    /// Will Return All Books Or EmptyList.
    /// </summary>
    [HttpGet]
    public IActionResult GetBooks()
    {
        CreateTables();

        using SQLiteConnection conn = new SQLiteConnection(connectionString);
        conn.Open();

        var books = new List<Book>();
        using (var command = new SQLiteCommand("SELECT * FROM Books",conn))

        using (SQLiteDataReader reader = command.ExecuteReader())
        {
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    
                    var book =  new Book
                    {
                        Id = Convert.ToInt64(reader["Id"]),
                        Name = (string)reader["Name"],
                        AuthorName = (string)reader["AuthorName"],
                        WriterId = (long)reader["WriterId"],
                        Price = Convert.ToSingle(reader["Price"]),
                    };

                    book.Genres = GetGenres(book.Id, conn);

                    books.Add(book);
                }
            }
            
        }

        return Ok(books);
    }
    
    /// <summary>
    /// Will return a book or not found result.
    /// </summary>
    /// <param name="id">This is book's id.</param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Book),200)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetBook(long id)
    {
        CreateTables();

        using SQLiteConnection conn = new SQLiteConnection(connectionString);
        conn.Open();

        Book book = null;
        string selectBookQuery = "SELECT * FROM Books WHERE Id = @bookId";
        using (SQLiteCommand selectBookCommand = new SQLiteCommand(selectBookQuery, conn))
        {
            selectBookCommand.Parameters.AddWithValue("@bookId", id);

            using (SQLiteDataReader reader = selectBookCommand.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    reader.Read();
                    book = new Book
                    {
                        Id = Convert.ToInt64(reader["Id"]),
                        Name = (string)reader["Name"],
                        AuthorName = (string)reader["AuthorName"],
                        WriterId = (long)reader["WriterId"],
                        Price = Convert.ToSingle(reader["Price"]),
                        Genres = GetGenres(id, conn)
                    };

                    return Ok(book);
                }
                else
                {
                    return NotFound(book);
                }
              
            }
        }
    }

    /// <summary>
    /// Will Update Book with the GivenId
    /// </summary>
    /// <param name="id"></param>
    /// <param name="updateModel"></param>
    [HttpPut("{id}")]
    public IActionResult UpdateBook(long id, CreateBookDTO updateModel)
    {
        CreateTables();

        using SQLiteConnection connection = new SQLiteConnection(connectionString);
        connection.Open();

        string updateQuery = $"UPDATE Books SET Name = @name, Price = @price, AuthorName = @author WHERE Id = {id}";

        using SQLiteCommand command = new SQLiteCommand(updateQuery, connection);
        command.Parameters.AddWithValue("@name", updateModel.Name);
        command.Parameters.AddWithValue("@price", updateModel.Price);
        command.Parameters.AddWithValue("@author", updateModel.AuthorName);

        int rowsAffected = command.ExecuteNonQuery();

        if (rowsAffected > 0)
        {
            string selectQuery = $"SELECT * FROM Books WHERE Id = {id}";

            using SQLiteCommand selectCommand = new SQLiteCommand(selectQuery, connection);
            using SQLiteDataReader reader = selectCommand.ExecuteReader();

            if (reader.Read())
            {
                Book updatedModel = new Book
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Name = reader["Name"].ToString(),
                    Price = Convert.ToSingle(reader["Price"]),
                    AuthorName = reader["AuthorName"].ToString()
                };

                return Ok(updatedModel);
            }
        }
        else
        {
            return NotFound();
        }

        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteBook(long id)
    {
        CreateTables();

        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();

            string deleteQuery = "DELETE FROM Books WHERE Id = @id";
            using (var command = new SQLiteCommand(deleteQuery, connection))
            {
                command.Parameters.AddWithValue("@id", id);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    return Ok(true);
                }
            }
        }

        return NotFound(false);
    }

}