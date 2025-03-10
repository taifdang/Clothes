namespace Clothes_BE.Models
{
    public class CartItems
    {
        public int id { get; set; }
        public int cart_id { get; set; }
        public int product_variant_id { get; set; }
        public int quantity { get; set; }
        public Carts carts { get; set; }
        public ProductVariants product_variants { get; set; }

    }
}
