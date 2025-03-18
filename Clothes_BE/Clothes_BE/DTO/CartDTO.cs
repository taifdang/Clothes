namespace Clothes_BE.DTO
{
    public class CartDTO
    {
        public int? id { get; set; }
        //public int cart_id { get; set; }
        public int product_variant_id { get; set; }
        public int quantity { get; set; }
    }
}
