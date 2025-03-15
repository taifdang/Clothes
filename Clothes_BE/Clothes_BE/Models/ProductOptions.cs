using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Clothes_BE.Models
{
    public class ProductOptions
    {     
        public int product_id { get; set; }     
        public string option_id { get; set; }
        [JsonIgnore]  
        
        public Products products { get; set; }
        [JsonIgnore]
        public Options options { get; set; }
    }
}
