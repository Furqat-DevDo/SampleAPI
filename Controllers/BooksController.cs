using FirstWeb.DTOS;
using FirstWeb.Models;
using FirstWeb.Services;
using FirstWeb.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Data.SQLite;

namespace FirstWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;
    public BooksController()
    {
       _bookService = new BookService();
    }

    /// <summary>
    /// Will Create new book with new genre.
    /// </summary>
    /// <param name="bookModel">new Book model. </param>
    [HttpPost]
    [ProducesResponseType(typeof(Book),200)]
    public IActionResult CreateBook(CreateBookModel bookModel)
    {
       return Ok(_bookService.Create(bookModel));
    }

   ///// <summary>
   ///// Wiil return all books or empty list.
   ///// </summary>
   // [HttpGet]
   // public IActionResult GetBooks()
   // {
   //     CreateTables();

   //     using SQLiteConnection conn = new(connectionString);
   //     conn.Open();

   //     var books = new List<Book>();
   //     using (var command = new SQLiteCommand("SELECT * FROM Books",conn))

   //     using (SQLiteDataReader reader = command.ExecuteReader())
   //     {
   //         if (reader.HasRows)
   //         {
   //             while (reader.Read())
   //             {
                    
   //                 var book =  new Book
   //                 {
   //                     Id = Convert.ToInt64(reader["Id"]),
   //                     Name = (string)reader["Name"],
   //                     AuthorName = (string)reader["AuthorName"],
   //                     WriterId = (long)reader["WriterId"],
   //                     Price = Convert.ToSingle(reader["Price"]),
   //                 };

   //                 book.Genres = GetGenres(book.Id, conn);

   //                 books.Add(book);
   //             }
   //         }
            
   //     }

   //     return Ok(books);
   // }
    
   // /// <summary>
   // /// Will return a book with the given Id.
   // /// </summary>
   // /// <param name="id">book's id</param>
   // [HttpGet("{id}")]
   // [ProducesResponseType(typeof(Book),200)]
   // [ProducesResponseType(StatusCodes.Status404NotFound)]
   // public IActionResult GetBook(long id)
   // {
   //     CreateTables();

   //     using SQLiteConnection conn = new(connectionString);
   //     conn.Open();

   //     Book book = new();
   //     string selectBookQuery = "SELECT * FROM Books WHERE Id = @bookId";
   //     using SQLiteCommand selectBookCommand = new(selectBookQuery, conn);
   //     selectBookCommand.Parameters.AddWithValue("@bookId", id);

   //     using SQLiteDataReader reader = selectBookCommand.ExecuteReader();
   //     if (reader.HasRows)
   //     {
   //         reader.Read();
   //         book = new Book
   //         {
   //             Id = Convert.ToInt64(reader["Id"]),
   //             Name = (string)reader["Name"],
   //             AuthorName = (string)reader["AuthorName"],
   //             WriterId = (long)reader["WriterId"],
   //             Price = Convert.ToSingle(reader["Price"]),
   //             Genres = GetGenres(id, conn)
   //         };

   //         return Ok(book);
   //     }
   //     else
   //     {
   //         return NotFound(null);
   //     }
   // }

   // /// <summary>
   // /// Will update the book with the given Id.
   // /// </summary>
   // /// <param name="id">Book id</param>
   // /// <param name="updateModel">Book Model</param>
   // /// <returns></returns>
   // [HttpPut("{id}")]
   // public IActionResult UpdateBook(long id, CreateBookModel updateModel)
   // {
   //     CreateTables();

   //     using SQLiteConnection connection = new(connectionString);
   //     connection.Open();

   //     string updateQuery = $"UPDATE Books SET Name = @name, Price = @price, AuthorName = @author WHERE Id = {id}";

   //     using SQLiteCommand command = new(updateQuery, connection);
   //     command.Parameters.AddWithValue("@name", updateModel.Name);
   //     command.Parameters.AddWithValue("@price", updateModel.Price);
   //     command.Parameters.AddWithValue("@author", updateModel.AuthorName);

   //     int rowsAffected = command.ExecuteNonQuery();

   //     if (rowsAffected > 0)
   //     {
   //         string selectQuery = $"SELECT * FROM Books WHERE Id = {id}";

   //         using SQLiteCommand selectCommand = new(selectQuery, connection);
   //         using SQLiteDataReader reader = selectCommand.ExecuteReader();

   //         if (reader.Read())
   //         {
   //             Book updatedModel = new()
   //             {
   //                 Id = (long)reader["Id"],
   //                 Name =(string)reader["Name"],
   //                 Price = (float)reader["Price"],
   //                 AuthorName =(string)reader["AuthorName"]
   //             };

   //             return Ok(updatedModel);
   //         }
   //     }
   //     else
   //     {
   //         return NotFound();
   //     }

   //     return new StatusCodeResult(StatusCodes.Status500InternalServerError);
   // }

   ///// <summary>
   ///// Will delete the book with the given Id.
   ///// </summary>
   ///// <param name="id">Book id</param>
   // [HttpDelete("{id}")]
   // [ProducesResponseType(typeof(bool),200)]
   // [ProducesResponseType(typeof(bool),400)]
   // public IActionResult DeleteBook(long id)
   // {
   //     CreateTables();

   //     using (var connection = new SQLiteConnection(connectionString))
   //     {
   //         connection.Open();

   //         string deleteQuery = "DELETE FROM Books WHERE Id = @id";
   //         using var command = new SQLiteCommand(deleteQuery, connection);
   //         command.Parameters.AddWithValue("@id", id);

   //         int rowsAffected = command.ExecuteNonQuery();

   //         if (rowsAffected > 0)
   //         {
   //             return Ok(true);
   //         }
   //     }

   //     return NotFound(false);
   // }

}