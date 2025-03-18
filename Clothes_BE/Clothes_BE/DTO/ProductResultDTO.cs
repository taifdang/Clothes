using Clothes_BE.Models;

namespace Clothes_BE.DTO
{
    public class ProductResultDTO
    {
        public int id { get; set; }
        public int category_id { get; set; }
        public string title { get; set; }
        public double price { get; set; }
        public double old_price { get; set; }
        public string description { get; set; }
        public List<ValueMapDTO> options { get; set; }
    }
}
