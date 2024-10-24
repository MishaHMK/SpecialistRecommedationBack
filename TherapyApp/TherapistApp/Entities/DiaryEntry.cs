namespace TherapyApp.Entities
{
    public class DiaryEntry
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool? IsPrivate { get; set; }
        public int DiaryId { get; set; }
        public Diary Diary { get; set; } = null!;
        public int EmotionId { get; set; }
        public Emotion Emotion { get; set; } = null!;
        public double Value { get; set; }
    }
}
