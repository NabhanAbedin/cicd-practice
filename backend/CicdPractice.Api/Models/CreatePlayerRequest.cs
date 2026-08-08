using System.ComponentModel.DataAnnotations;
using CicdPractice.Api.Entities;

namespace CicdPractice.Api.Models;

public class CreatePlayerRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Range(1, 99)]
    public int JerseyNumber { get; set; }

    public PlayerPosition Position { get; set; }
}
