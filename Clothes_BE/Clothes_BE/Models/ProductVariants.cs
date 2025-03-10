using System.Text.Json.Serialization;

namespace Clothes_BE.Models
{
    public class ProductVariants
    {
        public int id { get; set; }
        public int product_id { get; set; }
        public string title { get; set; } // variant label =>option1-option2(int or string) lấy từ form , check với bảng variant_value
        public double price { get; set; }
        public double old_price { get; set; }
        public int quantity { get; set; }                  
        public double percent { get; set; }
        public string sku { get; set; } //label(catetory)-id(product)-color-size(int or string)
                                        //label(catetory)-id(product)-color => image

        [JsonIgnore]
        public Products products { get; set; }    
        public ICollection<Variants> variants { get; set; }
        public ICollection<CartItems> cart_items { get; set; }
        public ICollection<OrderDetail> order_detail { get; set; }

    }
}
