using FirstWeb.Models;

namespace FirstWeb.DTOS
{
    public class CreateWriterDTO
    {
        public string FullName { get; set; }
        public DateTime BirthDate { get; set; }
        public List<Book> Books { get; set; }
    }
}
