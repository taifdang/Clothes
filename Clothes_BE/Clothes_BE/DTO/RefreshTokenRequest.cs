namespace Clothes_BE.DTO
{
    public class RefreshTokenRequest
    {
        public int user_id { get; set; }
        public string RefreshToken { get; set; }
    }
}
