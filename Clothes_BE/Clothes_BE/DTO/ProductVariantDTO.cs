namespace Clothes_BE.DTO
{
    public class ProductVariantDTO
    {
        public int id { get; set; }
        public int product_id { get; set; }
        //public string title { get; set; }
        public double price { get; set; }
        public double old_price { get; set; }      
         
        //public string product_title { get; set; }
        public int quantity { get; set; }
        public List<int> options { get; set; }
        //public double percent { get; set; }

    }
}
