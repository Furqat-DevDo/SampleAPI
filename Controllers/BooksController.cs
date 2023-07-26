using FirstWeb.DTOS;
using FirstWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace FirstWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BooksController : ControllerBase
{
    private static int count = 0;
    private static List<Book> Books = new ();

    /// <summary>
    /// You Can create a new book Model.
    /// </summary>
    /// <param name="bookModel"></param>
    /// <remarks>
    /// POST/TODO
    ///       {
    ///          "name": "Oq Kema",
    ///          "price": 25.02,
    ///          "authorName": "Cho'lpon",
    ///          "writerId": 12,
    ///          "genre": [0,1]
    ///       }
    /// </remarks>
    /// <response code="200">Returns the newly created book</response>
    [HttpPost]
    [ProducesResponseType(typeof(Book),200)]
    public IActionResult CreateBook(CreateBookDTO bookModel)
    {
        var newBook = new Book()
        {
            Id = count++,
            AuthorName = bookModel.AuthorName,
            Name = bookModel.Name,
            Genres = bookModel.Genre,
            Price = bookModel.Price,
            WriterId = bookModel.WriterId
        };
        
        Books.Add(newBook);
        
        return Ok(newBook);
    }

    [HttpGet]
    public IActionResult GetBooks()
    {
        return Ok(Books);
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
        var book = Books.FirstOrDefault(x => x.Id == id);
        return book is null ? NotFound() :  Ok(book);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateBook(int id,CreateBookDTO updateModel)
    {
        var book = Books.FirstOrDefault(x => x.Id == id);
        
        if (book == null) return NotFound($"Book with id: {id} not found");
        
        book.Name = updateModel.Name;
        book.AuthorName = updateModel.AuthorName;
        book.WriterId = updateModel.WriterId;
        book.Genres = updateModel.Genre;
        book.Price = updateModel.Price;
        
        return Ok(book);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteBook(int id)
    {
        var book = Books.FirstOrDefault(x => x.Id == id);
        if (book == null)
        {
            return NotFound("false");
        }
        Books.Remove(book);
        return Ok(true);
    }
}