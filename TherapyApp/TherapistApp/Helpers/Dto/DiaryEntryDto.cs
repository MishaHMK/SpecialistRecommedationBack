using TherapyApp.Entities;

namespace TherapyApp.Helpers.Dto;

public class DiaryEntryDto
{
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public int DiaryId { get; set; }
    public int EmotionId { get; set; }
    public double Value { get; set; }
}
