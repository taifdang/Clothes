using AutoMapper;
using System.Text.Json.Serialization;

namespace Clothes_BE.Models
{
    public class CartItems
    {
        public int id { get; set; }
        public int cart_id { get; set; }
        public int product_variant_id { get; set; }
        public int quantity { get; set; }
        [JsonIgnore]
        public Carts carts { get; set; }
        [JsonIgnore]
        public ProductVariants product_variants { get; set; }
       
        

    }
    
}
