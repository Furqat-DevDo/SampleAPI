using FirstWeb.DTOS;
using FirstWeb.Models;

namespace FirstWeb.Services.Interfaces;

public interface IWriterService
{
    public WriterModel Create(CreateWriterModel createWriterModel);
    public WriterModel Update(long id, CreateWriterModel updateWriterModel);
    public bool Delete(long id);
    public WriterModel GetById(long id);
    public List<WriterModel> GetAll();
}
