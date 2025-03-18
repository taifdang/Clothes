namespace Clothes_BE.Models
{
    public class Carts
    {
        public int id { get; set; }
        public string? session_id { get; set; }
        public int? user_id { get; set; }
        public Users users { get; set; }
        public ICollection<CartItems> cartItems { get; set; }
    }
}
