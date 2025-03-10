using System.Text.Json.Serialization;

namespace Clothes_BE.Models
{
    public class OptionValues
    {
        public int id {get;set;}
        public string option_id { get; set; }
        public string value { get; set; }
        public string? label { get; set; }
        [JsonIgnore]
        public Options options { get; set; }
        public ICollection<ProductOptionImages> product_option_images { get; set; }
        public ICollection<Variants> variants { get; set; }

    }
}
