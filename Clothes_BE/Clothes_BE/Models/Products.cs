using System.Text.Json.Serialization;

namespace Clothes_BE.Models
{
    public class Products
    {
        public int id { get; set; }
        public int category_id { get; set; }
        public string title { get; set; }
        public double price { get; set; }
        public double old_price { get; set; }
        public string? description { get; set; }
            
        //constraint    
        [JsonIgnore]
        public Categories categories { get; set; }
        public ICollection<ProductOptions> product_options { get; set; }
        public ICollection<ProductVariants> product_variants { get; set; }     
        public ICollection<ProductOptionImages> product_option_images { get; set; }




    }
}
