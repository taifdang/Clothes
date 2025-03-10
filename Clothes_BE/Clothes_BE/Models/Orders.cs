namespace Clothes_BE.Models
{
    public class Orders
    {
        public int id { get; set; }
        public int user_id { get; set; }
        public string status { get; set; }
        public string? note { get; set; }
        public double total { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public Users users { get; set; }
        public ICollection<OrderDetail> order_detail { get; set; }
    }
}
