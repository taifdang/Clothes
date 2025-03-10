using System.Text.Json.Serialization;

namespace Clothes_BE.Models
{
    public class ProductOptionImages
    {
        public int id { get; set; }
        public int product_id { get; set; }
        public int option_value_id { get; set; }
        public string? src { get; set; }
        [JsonIgnore]
        public Products products { get; set; }
        [JsonIgnore]
        public OptionValues options_values { get; set; }

    }
}
