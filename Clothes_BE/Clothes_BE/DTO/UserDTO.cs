using System.ComponentModel.DataAnnotations;

namespace Clothes_BE.DTO
{
    public class UserDTO
    {
        public int id { get; set; }
        [EmailAddress]
        public string email { get; set; }
        public string? name { get; set; }
        public string? avatar { get; set; }
        [Phone]
        public string? phone { get; set; }
        [DataType(DataType.Password)]
        public string password { get; set; }
        [Compare("password", ErrorMessage ="Not match")]
        [Required]
        [Display(Name ="Comfirm Password")]
        [DataType(DataType.Password)]
        public string confirm_password { get; set; }
        
       
    }
}
