namespace Clothes_BE.DTO
{
    public class ProductDTO
    {
        public int id { get; set; }
        public string tilte { get; set; }
        public string slug { get; set; }
        public List<ProductImageDTO> images { get; set; }
        public double price { get; set; }
        public double old_price { get; set; }
        public List<ProductVariantDTO> variants { get; set; }
    }
}
