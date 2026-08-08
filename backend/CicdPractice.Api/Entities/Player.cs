namespace CicdPractice.Api.Entities;

public class Player
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int JerseyNumber { get; set; }
    public PlayerPosition Position { get; set; }
    public LineupStatus LineupStatus { get; set; } = LineupStatus.Reserve;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
