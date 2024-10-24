using Microsoft.AspNetCore.Identity;

namespace TherapyApp.Entities
{
    public class AppUser : IdentityUser<string>
    {
        public string? FirstName { get; set; } = null!;
        public string? LastName { get; set;} = null!;
        public string? FatherName { get; set; } = null!;
        public DateTime? RegisteredOn { get; set; }
        public DateTime? LastActive { get; set; }
        public Therapist? TherapistUser { get; set; }
    }
}
