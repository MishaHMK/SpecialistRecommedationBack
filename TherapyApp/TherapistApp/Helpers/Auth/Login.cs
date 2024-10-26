using System.ComponentModel.DataAnnotations;

namespace TherapyApp.Helpers.Auth
{
    public class Login
    {
        [Required]
        public string Email { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
    }
}
