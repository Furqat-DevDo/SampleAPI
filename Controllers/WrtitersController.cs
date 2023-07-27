using FirstWeb.DTOS;
using FirstWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace FirstWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class WrtitersController : ControllerBase
{
    private static int count = 1;
    private static List<Writer> Writers = new();

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
    public IActionResult CreateWriter(CreateWriterDTO writerModel)
    {
        var writer = new Writer()
        {
            Id = count++,
            FullName = writerModel.FullName,
            BirthDate = writerModel.BirthDate,
            Books = writerModel.Books
        };

        Writers.Add(writer);
        return Ok(writer);

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
    public IActionResult PutWriter(int id, CreateWriterDTO writerModel)
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