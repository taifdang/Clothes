namespace Clothes_BE.Models
{
    public class OrderDetail
    {
        public int id { get; set; }
        public int order_id { get; set; }
        public int product_variant_id { get; set; }
        public double price { get; set; }
        public int quantity { get; set; }
        public Orders orders { get; set; }
        public ProductVariants product_variants { get; set; }
    }
}
