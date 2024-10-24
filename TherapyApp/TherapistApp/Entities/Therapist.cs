using System.ComponentModel.DataAnnotations;

namespace TherapyApp.Entities
{
    public class Therapist
    {
        public int Id { get; set; }
        public string Introduction { get; set; } = null!;
        public int? SpecialityId { get; set; }
        public Speciality Speciality { get; set; } = null!;

        [Required]
        public string? UserId { get; set; }
        public AppUser? User { get; set; }
    }
}
