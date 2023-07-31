using FirstWeb.Models;

namespace FirstWeb.DTOS
{
    public class CreateWriterModel
    {
        public string FullName { get; set; }
        public DateTime BirthDate { get; set; }
        public List<Book> Books { get; set; }
    }
}
