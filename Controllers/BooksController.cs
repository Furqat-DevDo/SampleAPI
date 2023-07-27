using FirstWeb.DTOS;
using FirstWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SQLite;

namespace FirstWeb.Controllers;

/// <summary>
/// 
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class updateModelsController : ControllerBase
{
    // Connection string to the SQLite database file.
    private const string connectionString = "Data Source= mydatabase.db;Version=3;";
    //Query which will create Table
    private readonly List<string> createTableQueries = new ()
    {
        "CREATE TABLE IF NOT EXISTS updateModels (Id INTEGER PRIMARY KEY, Name TEXT, Price REAL, AuthorName TEXT,WriterId INTEGER)",
        "CREATE TABLE Genres(Id INT PRIMARY KEY,Name NVARCHAR(100) NOT NULL);",
        "CREATE TABLE updateModelGenres(updateModelId INT NOT NULL,GenreId INT NOT NULL,PRIMARY KEY (updateModelId, GenreId),FOREIGN KEY (updateModelId) " +
        "REFERENCES updateModels (Id),FOREIGN KEY (GenreId) REFERENCES Genres (Id));"
    };


    private void CreateTables()
    {
        using SQLiteConnection connection = new SQLiteConnection(connectionString);

        connection.Open();

        foreach (var query in createTableQueries)
        {
            using SQLiteCommand command = new SQLiteCommand(query, connection);
            command.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// You Can create a new updateModel Model.
    /// </summary>
    /// <param name="updateModelModel"></param>
    /// <remarks>
    /// POST/TODO
    /// {
    ///    "name": "Oq Kema",
    ///    "price": 25.02,
    ///    "authorName": "Cho'lpon",
    ///    "writerId": 12,
    ///    "genre": [0,1]
    /// }
    /// </remarks>
    /// <response code="200">Returns the newly created book</response>
    [HttpPost]
    [ProducesResponseType(typeof(Book),200)]
    public IActionResult CreateupdateModel(CreateBookDTO updateModelModel)
    {
        CreateTables();

        using SQLiteConnection connection = new SQLiteConnection(connectionString);
        connection.Open();

        string insertQuery = "INSERT INTO updateModels (Name, Price, AuthorName,WriterId) VALUES (@name, @price, @author,@writerId)";

        using (SQLiteCommand command = new SQLiteCommand(insertQuery, connection))
        {
            command.Parameters.AddWithValue("@name", $"{updateModelModel.Name}");
            command.Parameters.AddWithValue("@price", updateModelModel.Price);
            command.Parameters.AddWithValue("@author", $"{updateModelModel.AuthorName}");
            command.Parameters.AddWithValue("@writerId",$"{updateModelModel.WriterId}");
            command.ExecuteNonQuery();
        }

        return Ok();
    }

    [HttpGet]
    public IActionResult GetupdateModels()
    {
        using SQLiteConnection connection = new SQLiteConnection(connectionString);
        connection.Open();

        string selectQuery = "SELECT * FROM Books";

        var result = new List<Book>();
        using (SQLiteCommand command = new SQLiteCommand(selectQuery, connection))
        {
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add(new Book
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Name = reader["Name"].ToString(),
                        Price = (float)(reader["Price"]),
                        AuthorName = reader["AuthorName"].ToString()
                    });
                }
            }
        }

        return Ok(result);
    }
    
    /// <summary>
    /// Will return a updateModel or not found result.
    /// </summary>
    /// <param name="id">This is updateModel's id.</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Book),200)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetupdateModel(int id)
    {
        using (SQLiteConnection connection = new SQLiteConnection("Data Source=mydatabase.db;Version=3;"))
        {
            connection.Open();

            string selectQuery = "SELECT * FROM Books WHERE Id = @id";
            using (SQLiteCommand command = new SQLiteCommand(selectQuery, connection))
            {
                command.Parameters.AddWithValue("@id", id);

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Book book = new Book
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Name = reader["Name"].ToString(),
                            Price = Convert.ToSingle(reader["Price"]),
                            AuthorName = reader["AuthorName"].ToString()
                        };

                        return Ok(book);
                    }
                }
            }
        }

        return NotFound();
    }

    [HttpPut("{id}")]
    public IActionResult UpdateupdateModel(int id, CreateBookDTO updateModel)
    {
        using (SQLiteConnection connection = new SQLiteConnection("Data Source=mydatabase.db;Version=3;"))
        {
            connection.Open();

            string updateQuery = "UPDATE updateModels SET Name = @name, Price = @price, AuthorName = @author "
                               + $"WHERE Id = {id}";

            using (SQLiteCommand command = new SQLiteCommand(updateQuery, connection))
            {
                command.Parameters.AddWithValue("@name", updateModel.Name);
                command.Parameters.AddWithValue("@price", updateModel.Price);
                command.Parameters.AddWithValue("@author", updateModel.AuthorName);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    string selectQuery = "SELECT * FROM updateModels WHERE Id = @id";

                    using (SQLiteCommand selectCommand = new SQLiteCommand(selectQuery, connection))
                    {
                        using (SQLiteDataReader reader = selectCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Book updatedupdateModel = new Book
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Name = reader["Name"].ToString(),
                                    Price = Convert.ToSingle(reader["Price"]),
                                    AuthorName = reader["AuthorName"].ToString()
                                };

                                return Ok(updatedupdateModel);
                            }
                        }
                    }
                }
                else
                {
                    return NotFound();
                }
            }
        }

        return NotFound();
    }


    [HttpDelete("{id}")]
    public IActionResult DeleteupdateModel(int id)
    {
        using (SQLiteConnection connection = new SQLiteConnection("Data Source=mydatabase.db;Version=3;"))
        {
            connection.Open();

            string deleteQuery = "DELETE FROM Books WHERE Id = @id";
            using (SQLiteCommand command = new SQLiteCommand(deleteQuery, connection))
            {
                command.Parameters.AddWithValue("@id", id);

                int rowsAffected = command.ExecuteNonQuery();

                connection.Close();

                return rowsAffected > 0 ? Ok(true) : NotFound(false);
            }
        }
    }
}