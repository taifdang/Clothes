namespace Clothes_BE.DTO
{
    public class ProductImageDTO
    {
        
        public int product_id { get; set; }
        public int option_value_id { get; set; }
        public IFormFile[]? files { get; set; }
    }
}
