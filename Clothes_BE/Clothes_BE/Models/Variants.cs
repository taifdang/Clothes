using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Clothes_BE.Models
{
    public class Variants
    {
        
        public int product_variant_id { get; set; }
        public int option_value_id { get; set; }
        [JsonIgnore]
        public ProductVariants product_variants { get; set; }
        [JsonIgnore]
        public OptionValues option_values { get; set; }
        
    }
}
