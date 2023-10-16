using System.ComponentModel.DataAnnotations;

namespace webapp.Models;

public class CTCList
{
    [Required]
    public List<CardTypeCount> CTCs { get; set; } = new List<CardTypeCount>();
}