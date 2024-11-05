namespace TherapyApp.Entities
{
    public class Meeting
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Url { get; set; }
        public DateTime? StartDate { get; set; }
        public bool? IsOnline { get; set; }
        public bool? IsCancelled { get; set; }

        public string ClientId { get; set; } = null!;
        public AppUser Client { get; set; } = null!;

        public string TherapistId { get; set; } = null!;
        public AppUser Therapist { get; set; } = null!;
    }
}
