using System.Text.Json.Serialization;

namespace Clothes_BE.Models
{
    public class Categories
    {
        public int id { get; set; }
        public int product_types_id { get; set; }
        public string value { get; set; }
        public string? label { get; set; }

        //constraint
        [JsonIgnore]
        public ProductTypes product_types { get; set; }
        public ICollection<Products> products { get; set; }
    }
}
