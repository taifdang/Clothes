namespace Clothes_BE.Models
{
    public class Options
    {
        public string id { get; set; }
        public string title { get; set; }
        public ICollection<ProductOptions> product_options { get; set; }
        public ICollection<OptionValues> option_values { get; set; }
    }
}
