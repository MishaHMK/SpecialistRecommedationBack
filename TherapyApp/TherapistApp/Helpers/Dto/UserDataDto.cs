using System.ComponentModel.DataAnnotations;

namespace TherapyApp.Helpers.Dto;

public class UserDataDto
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = null!;
    [Required]
    [MaxLength(50)]
    public string Surname { get; set; } = null!;
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}