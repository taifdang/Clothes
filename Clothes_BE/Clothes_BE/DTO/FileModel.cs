namespace Clothes_BE.DTO
{
    public class FileModel
    {
        public int id { get; set; }
        public int product_id { get; set; }
        public int option_value_id { get; set; }
        public IFormFile? files { get; set; }
    }
}
