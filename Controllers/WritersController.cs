using FirstWeb.DTOS;
using FirstWeb.Models;
using FirstWeb.Services.Interfaces;
using FirstWeb.Services;
using Microsoft.AspNetCore.Mvc;
using System.Data.SQLite;
using System.Data.SqlClient;

namespace FirstWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class WritersController : ControllerBase
{
    private readonly IWriterService _writerService;

    public WritersController()
    {
        _writerService = new WriterService();
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
    ///      }       
    /// </remarks>
    /// <response code="200">Returns the newly created Writer</response>

    [HttpPost]
    [ProducesResponseType(typeof(Writer),200)]
    public IActionResult CreateWriter(CreateWriterModel writerModel)
    {
        try
        {
            var result  = _writerService.Create(writerModel);
            return result is null ? 
            new StatusCodeResult(StatusCodes.Status500InternalServerError) 
            : Ok(result);
        }
        catch(SqlException)
        {
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
        catch(Exception ex)
        {
            return BadRequest(ex.Message);
        }   
    }

    /// <summary>
    /// Will return all writers.
    /// </summary>
    /// <param name="id">This is writer's id.</param>
    /// <returns></returns>
    [HttpGet]
    public IActionResult GetWriters()
    {
        return Ok(_writerService.GetAll());
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
        var result = _writerService.GetById(id);
        return result is not null ? Ok(result) : NotFound(result);
    }

    [HttpPut("{id}")]
    public IActionResult PutWriter(int id, CreateWriterModel writerModel)
    {
        var result = _writerService.Update(id, writerModel);

        return result is not null ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteWriter(int id)
    {
        var result = _writerService.Delete(id);

        return result ? Ok(result) : NotFound(result);
    }
}