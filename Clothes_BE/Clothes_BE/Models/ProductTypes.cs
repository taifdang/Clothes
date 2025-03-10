namespace Clothes_BE.Models
{
    public class ProductTypes
    {
        public int id { get; set; }
        public string title { get; set; }
        public string? label { get; set; }
        //constraint   
        public ICollection<Categories> categories { get; set; }

    }
}
