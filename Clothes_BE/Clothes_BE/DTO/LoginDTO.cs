using System.ComponentModel.DataAnnotations;

namespace Clothes_BE.DTO
{
    public class LoginDTO
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string password { get; set; }
    }
}
