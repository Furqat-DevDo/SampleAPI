using System.Data.SqlClient;
using System.Data.SQLite;
using FirstWeb.DTOS;
using FirstWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace FirstWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenresController : ControllerBase
    {
        private readonly ILogger<GenresController> _logger;

        public GenresController(ILogger<GenresController> logger)
        {
            _logger= logger;
        }
        private const string connectionString = "Data Source=mydata.db; Version=3;";
        

        private void CreateTable()
        {
            var query = "CREATE TABLE IF NOT EXISTS Genres(Id INTEGER PRIMARY KEY AUTOINCREMENT, Name NVARCHAR(100) NOT NULL);";
            var connection = new SQLiteConnection(connectionString);
            connection.Open();
            var command = new SQLiteCommand(query, connection);
            command.ExecuteNonQuery();
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            CreateTable();
            return new string[] { "value1", "value2" };
        }

       
        [HttpGet("{id}")]
        public string Get(int id)
        {
            CreateTable();
            return "value";
        }

        
        [HttpPost]
        public IActionResult Post([FromBody] CreateGenreModel dto)
        {
            CreateTable();

            try
            {
                using var connection = new SQLiteConnection(connectionString);
                connection.Open();

                var query = "INSERT INTO Genres (Name) VALUES (@name)";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@name", $"{dto.Name}");

                command.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            try
            {
                using var connection2 = new SQLiteConnection(connectionString);
                connection2.Open();
                string selectQuery = "SELECT * FROM Genres WHERE Name = @name";
                using SQLiteCommand command = new SQLiteCommand(selectQuery, connection2);
                command.Parameters.AddWithValue("@name", dto.Name);

                using SQLiteDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var genre = new Genre
                    {
                        Id = (long)reader["Id"],
                        Name = (string)reader["Name"],
                    };

                    return Ok(genre);
                }
            }
            catch (SQLiteException ex)
            {
                _logger.LogError(ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            
            _logger.LogError("Cannot Create Table");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
            CreateTable();
        }


        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            CreateTable();
        }
    }
}
