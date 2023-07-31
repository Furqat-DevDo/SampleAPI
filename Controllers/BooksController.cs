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

    /// <summary>
    /// Wiil return all books or empty list.
    /// </summary>
    [HttpGet]
    public IActionResult GetBooks()
    {
        return Ok(_bookService.GetAll());
    }

    /// <summary>
    /// Will return a book with the given Id.
    /// </summary>
    /// <param name="id">book's id</param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Book), 200)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetBook(long id)
    {
        var obj = _bookService.GetById(id);

        return obj != null ? Ok(obj) : NotFound(null);
    }

    /// <summary>
    /// Will update the book with the given Id.
    /// </summary>
    /// <param name="id">Book id</param>
    /// <param name="updateModel">Book Model</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public IActionResult UpdateBook(long id, CreateBookModel updateModel)
    {
        var obj = _bookService.Update(id, updateModel);

        return obj != null ? Ok(obj) : NotFound(null);
    }

    /// <summary>
    /// Will delete the book with the given Id.
    /// </summary>
    /// <param name="id">Book id</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(bool), 200)]
    [ProducesResponseType(typeof(bool), 400)]
    public IActionResult DeleteBook(long id)
    {
        var isDeleted = _bookService.Delete(id);
        
        return isDeleted ? Ok(isDeleted) : NotFound(isDeleted);
    }

}