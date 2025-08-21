public class CalendarEvent
{
    public string? Title { get; set; }
    public DateTime? BeginDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsOnline { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public List<string> Participants { get; set; } = [];
}