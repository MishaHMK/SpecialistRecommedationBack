namespace TherapyApp.Entities
{
    public class Diary
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public AppUser? User { get; set; }
        public ICollection<DiaryEntry>? Entries { get; set; }
    }
}
