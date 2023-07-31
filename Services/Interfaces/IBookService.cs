using FirstWeb.DTOS;
using FirstWeb.Models;

namespace FirstWeb.Services.Interfaces;

public interface IBookService
{
    public BookModel Create(CreateBookModel createBookModel);
    public BookModel Update(long id, CreateBookModel updateBookModel);
    public bool Delete(long id);
    public BookModel GetById(long id);
    public List<BookModel> GetAll();
}
