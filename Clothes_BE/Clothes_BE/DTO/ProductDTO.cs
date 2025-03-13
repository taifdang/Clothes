using Clothes_BE.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Clothes_BE.DTO
{
    public class ProductDTO
    {
        public int id { get; set; }
        public int category_id { get; set; }
        public string title { get; set; }      
        public double price { get; set; }
        public double old_price { get; set; }
        public string description { get; set; }
        public string option1 { get; set; }
        public string option2 { get; set; }
       
       
    
    }
}
